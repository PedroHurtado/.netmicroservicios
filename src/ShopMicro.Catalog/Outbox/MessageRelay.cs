namespace ShopMicro.Catalog.Outbox;

using Microsoft.EntityFrameworkCore;
using ShopMicro.Catalog.Persistence;
using ShopMicro.Domain;

/// <summary>
/// Drena el Outbox hacia el broker en segundo plano. La escritura en el Outbox (transaccional,
/// dentro del SaveChanges del caso de uso) y la publicación (aquí, reintentable) están
/// separadas a propósito: si el broker está caído, el evento no se pierde, queda Pending y se
/// reintenta en la siguiente vuelta. Esa separación es la razón de ser del patrón Outbox.
/// <para>
/// Garantía <b>at-least-once</b> (no exactly-once): si el proceso muere tras
/// <c>PublishAsync</c> y antes de <c>SaveChangesAsync</c>, el evento se republica. Los
/// consumidores deben ser idempotentes.
/// </para>
/// </summary>
public sealed class MessageRelay(IServiceProvider serviceProvider, IEventBus eventBus) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DrainOutboxAsync(stoppingToken);

            try
            {
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Apagado normal del host: salimos del bucle sin ruido.
                break;
            }
        }
    }

    private async Task DrainOutboxAsync(CancellationToken cancellationToken)
    {
        // Scope propio: el BackgroundService es singleton, el DbContext es scoped.
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        // 1. Leer los pendientes (orden de inserción aproximado → orden de publicación).
        var pending = await dbContext.Set<DomainEvent<Guid>>()
            .Where(domainEvent => domainEvent.Status == DomainEventStatus.Pending)
            .OrderBy(domainEvent => domainEvent.TimeStamp)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        // 2. Publicar cada uno al broker y marcarlo como publicado.
        foreach (var domainEvent in pending)
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
            domainEvent.MarkAsPublished();   // Pending → Publish
        }

        // 3. Persistir los cambios de Status.
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
