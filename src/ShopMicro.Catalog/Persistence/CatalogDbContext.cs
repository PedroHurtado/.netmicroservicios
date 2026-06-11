namespace ShopMicro.Catalog.Persistence;

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;
using ShopMicro.Catalog.Features.PizzasAgregate.Persistence;
using ShopMicro.Catalog.Outbox;
using ShopMicro.Domain;
using ShopMicro.Infrastructure;

/// <summary>
/// DbContext del microservicio Catalog. Persiste las <b>entidades EF</b> (la fila), nunca el
/// dominio: por eso los <c>DbSet</c> exponen los tipos <c>*Ef</c>. La excepción es el
/// <b>outbox</b>: el <see cref="DomainEvent{TId}"/> ES la fila, así que se persiste tal cual.
/// <para>
/// Además es la <b>unidad de trabajo que despacha</b> los eventos de dominio: como el
/// agregado no entra en el <c>ChangeTracker</c>, los repositorios se los entregan vía
/// <see cref="IDomainEventCollector"/> y aquí, dentro de <see cref="SaveChangesAsync"/> y
/// <b>antes del commit</b>, se publican con MediatR. Así la fila del outbox que escribe el
/// handler entra en la <b>misma transacción</b> que el cambio de estado: un único commit atómico.
/// </para>
/// </summary>
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options, IPublisher publisher)
    : DbContext(options), IDomainEventCollector
{
    private readonly IPublisher _publisher = publisher;
    private readonly List<IHasDomainEvents> _aggregatesWithEvents = [];

    public DbSet<IngredientEf> Ingredients => Set<IngredientEf>();

    public DbSet<PizzaEf> Pizzas => Set<PizzaEf>();

    public DbSet<DomainEvent<Guid>> Outbox => Set<DomainEvent<Guid>>();

    /// <summary>Los repositorios entregan aquí los agregados de dominio con eventos pendientes.</summary>
    public void Collect(IHasDomainEvents aggregate) => _aggregatesWithEvents.Add(aggregate);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        // 1. Congelar los agregados recogidos y vaciar el buzón: si un handler vuelve a
        //    llamar a SaveChanges en cadena, no reprocesamos lo mismo.
        var aggregates = _aggregatesWithEvents.ToList();
        _aggregatesWithEvents.Clear();

        // 2. Aplanar todos los eventos ANTES de limpiar los agregados.
        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        // 3. Limpiar los eventos del agregado antes de publicar (corta reprocesos en cadena;
        //    los eventos ya están a salvo en la lista materializada).
        aggregates.ForEach(aggregate => aggregate.Clear());

        // 4. Publicar cada evento. El handler del outbox escribe en ESTE mismo DbContext,
        //    de modo que su INSERT entra en el commit del base.SaveChangesAsync() final.
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IngredientEf>(entity =>
        {
            entity.HasKey(ingredient => ingredient.Id);
            entity.Property(ingredient => ingredient.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<PizzaEf>(entity =>
        {
            entity.HasKey(pizza => pizza.Id);
            entity.Property(pizza => pizza.Name).IsRequired().HasMaxLength(200);
            // IngredientIds es una colección primitiva (ids de otro agregado): sin navegación
            // ni tabla de unión hacia IngredientEf.
        });

        modelBuilder.Entity<DomainEvent<Guid>>(entity =>
        {
            // La fila del Outbox. (El nombre de tabla "Outbox" se fija con ToTable cuando haya
            // provider relacional; con InMemory no aplica.)
            entity.HasKey(domainEvent => domainEvent.Id);
            // El payload es object: lo serializamos a JSON usando su tipo en tiempo de ejecución
            // (si no, se serializaría como object → "{}"). Al releer queda como JSON crudo;
            // el mapeo fino a jsonb se detalla en el Módulo 6 (EF Core + PostgreSQL).
            entity.Property(domainEvent => domainEvent.Payload)
                .HasConversion(
                    payload => JsonSerializer.Serialize(payload, payload.GetType(), (JsonSerializerOptions?)null),
                    json => json);
        });
    }
}
