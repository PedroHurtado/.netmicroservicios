namespace ShopMicro.Catalog.Outbox;

using ShopMicro.Domain;

/// <summary>
/// Stub de <see cref="IEventBus"/> para el curso: en lugar de publicar a RabbitMQ, deja
/// constancia por log de que el evento habría salido al broker. Sustituible por la
/// implementación real (RabbitMQ + MassTransit) sin tocar el <see cref="MessageRelay"/>.
/// </summary>
public sealed class LoggingEventBus(ILogger<LoggingEventBus> logger) : IEventBus
{
    private readonly ILogger<LoggingEventBus> _logger = logger;

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class
    {
        if (@event is DomainEvent<Guid> domainEvent)
        {
            _logger.LogInformation(
                "EventBus → publicado al broker: {EventType} de {Aggregate} {IdAggregate}",
                domainEvent.EventType,
                domainEvent.Aggregate,
                domainEvent.IdAggregate);
        }
        else
        {
            _logger.LogInformation("EventBus → publicado al broker: {EventType}", typeof(TEvent).Name);
        }

        return Task.CompletedTask;
    }
}
