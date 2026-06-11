namespace ShopMicro.Domain;

/// <summary>
/// Raíz de agregado. Es una <see cref="EntityBase{TId}"/> que, además, <b>acumula</b> los
/// eventos de dominio que el agregado va generando. La colección se expone como solo lectura:
/// nadie de fuera la manipula directamente.
/// <para>
/// A diferencia del outbox, el agregado emite un <see cref="DomainEventData"/> ligero: solo
/// lo que conoce de verdad (tipo de evento, nombre e id del agregado, payload). Por eso esta
/// clase tiene <b>un único</b> parámetro de tipo: el id del evento del outbox no es asunto del
/// dominio, lo decide la infraestructura al persistir.
/// </para>
/// </summary>
/// <typeparam name="TId">tipo del identificador del agregado</typeparam>
public abstract class AggregateRoot<TId>(TId id) : EntityBase<TId>(id), IHasDomainEvents
{
    private readonly List<DomainEventData> _domainEvents = [];

    public IReadOnlyList<DomainEventData> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registra un evento de dominio. El agregado solo aporta el tipo de evento y el payload;
    /// el nombre del agregado y su identidad se derivan aquí. Los campos de infraestructura
    /// (usuario, timestamp, id del evento) los pone el handler, no el dominio.
    /// </summary>
    protected void AddEvent(string eventType, object payload)
        => _domainEvents.Add(new DomainEventData(eventType, GetType().Name, (Guid)(object)Id!, payload));

    public void Clear() => _domainEvents.Clear();
}
