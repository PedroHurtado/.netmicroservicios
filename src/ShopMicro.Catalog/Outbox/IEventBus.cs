namespace ShopMicro.Catalog.Outbox;

/// <summary>
/// Abstracción del broker. El <see cref="MessageRelay"/> depende de este contrato, no de la
/// tecnología concreta. La implementación real con RabbitMQ + MassTransit llega en el Módulo 6;
/// de momento un stub que registra por log es suficiente para cerrar el Camino 1 end-to-end.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class;
}
