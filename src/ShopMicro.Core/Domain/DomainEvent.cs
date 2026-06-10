namespace ShopMicro.Domain;

/// <summary>
/// Estado de publicación de un evento de dominio.
/// </summary>
public enum DomainEventStatus
{
    Pending,
    Publish
}

/// <summary>
/// Evento de dominio. Es a su vez una entidad (tiene identidad propia), por eso
/// extiende <see cref="EntityBase{TId}"/>. El tipo del identificador es genérico:
/// al persistirse en un outbox, cada microservicio elige el tipo de id que necesite
/// (no se asume <see cref="Guid"/>).
/// El payload puede ser un record, una clase o el propio agregado (this).
/// El constructor y los atributos son protected; los datos solo se exponen vía
/// getter para garantizar la inmutabilidad.
/// </summary>
/// <typeparam name="TId">tipo del identificador del evento (lo elige el outbox)</typeparam>
public class DomainEvent<TId> : EntityBase<TId>
{
    public string EventType { get; }

    public string Aggregate { get; }

    public Guid IdAggregate { get; }

    public string User { get; }

    public DateTime TimeStamp { get; }

    public DomainEventStatus Status { get; protected set; }

    public object Payload { get; }

    protected DomainEvent(
        TId id,
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

    public static DomainEvent<TId> Create(
        TId id,
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
