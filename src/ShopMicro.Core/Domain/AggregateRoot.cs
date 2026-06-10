namespace ShopMicro.Domain;

/// <summary>
/// Raíz de agregado. No es más que una <see cref="EntityBase{TId}"/> que, además,
/// acumula los eventos de dominio generados por el agregado. La colección se expone
/// como solo lectura para garantizar su inmutabilidad desde el exterior.
/// </summary>
/// <typeparam name="TId">tipo del identificador del agregado</typeparam>
/// <typeparam name="TEventId">
/// tipo del identificador de los eventos de dominio acumulados. Es independiente del
/// id del agregado: el outbox decide con qué tipo de id persiste cada evento.
/// </typeparam>
public abstract class AggregateRoot<TId, TEventId>(TId id) : EntityBase<TId>(id)
{
    private readonly List<DomainEvent<TEventId>> _domainEvents = [];

    public IReadOnlyList<DomainEvent<TEventId>> DomainEvents => _domainEvents.AsReadOnly();

    protected void Add(DomainEvent<TEventId> domainEvent) => _domainEvents.Add(domainEvent);

    protected void Remove(DomainEvent<TEventId> domainEvent) => _domainEvents.Remove(domainEvent);

    public void Clear() => _domainEvents.Clear();
}
