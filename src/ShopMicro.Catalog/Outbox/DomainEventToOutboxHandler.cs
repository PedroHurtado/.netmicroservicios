namespace ShopMicro.Catalog.Outbox;

using MediatR;
using ShopMicro.Catalog.Persistence;
using ShopMicro.Domain;

/// <summary>
/// Camino 1 — el handler que escribe en el Outbox. Es <b>genérico</b>: un único handler para
/// todos los eventos de dominio (no uno por tipo). Convierte el <see cref="DomainEventData"/>
/// que emitió el agregado en la fila real <see cref="DomainEvent{TId}"/> y rellena <b>aquí</b>
/// los campos de infraestructura que el dominio no conoce (id del evento, usuario, timestamp).
/// No hay commit: lo hará el <c>base.SaveChangesAsync()</c> del despacho, en la misma transacción.
/// </summary>
public sealed class DomainEventToOutboxHandler(CatalogDbContext dbContext)
    : INotificationHandler<DomainEventNotification>
{
    private readonly CatalogDbContext _dbContext = dbContext;

    public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        var data = notification.Data;

        // Campos de infraestructura hardcodeados de momento (no hay autenticación todavía).
        // Su sitio es el handler, no el dominio: poblar el User exigiría inyectar el Principal,
        // y eso es infraestructura.
        var outboxRow = DomainEvent<Guid>.Create(
            Guid.NewGuid(),
            data.EventType,
            data.Aggregate,
            data.IdAggregate,
            user: "system",
            timeStamp: DateTime.UtcNow,
            payload: data.Payload);

        _dbContext.Set<DomainEvent<Guid>>().Add(outboxRow);
        return Task.CompletedTask;
    }
}
