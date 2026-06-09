namespace Domain.Core;

/// <summary>
/// Estado de publicación de un evento de dominio.
/// </summary>
public enum DomainEventStatus
{
    Pending,
    Publish
}

/// <summary>
/// Evento de dominio. Es a su vez una entidad (tiene identidad propia).
/// El payload puede ser un record, una clase o el propio agregado (this).
/// El constructor y los atributos son protected; los datos solo se exponen
/// vía getter para garantizar la inmutabilidad.
/// </summary>
public class DomainEvent : EntityBase
{
    public string EventType { get; }

    public string Aggregate { get; }

    public Guid IdAggregate { get; }

    public string User { get; }

    public DateTime TimeStamp { get; }

    public DomainEventStatus Status { get; protected set; }

    public object Payload { get; }

    protected DomainEvent(
        Guid id,
        string eventType,
        string aggregate,
        Guid idAggregate,
        string user,
        DateTime timeStamp,
        object payload)
        : base(id)
    {
        EventType = eventType;
        Aggregate = aggregate;
        IdAggregate = idAggregate;
        User = user;
        TimeStamp = timeStamp;
        Status = DomainEventStatus.Pending;
        Payload = payload;
    }

    public static DomainEvent Create(
        Guid id,
        string eventType,
        string aggregate,
        Guid idAggregate,
        string user,
        DateTime timeStamp,
        object payload)
        => new(id, eventType, aggregate, idAggregate, user, timeStamp, payload);

    /// <summary>
    /// Marca el evento como publicado (pending -> publish).
    /// </summary>
    public void MarkAsPublished() => Status = DomainEventStatus.Publish;
}
