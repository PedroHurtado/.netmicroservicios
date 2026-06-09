namespace Domain.Core;

/// <summary>
/// Raíz de agregado. Hereda de <see cref="EntityBase"/> y gestiona la
/// colección de eventos de dominio generados por el agregado.
/// La colección se expone como solo lectura para garantizar su inmutabilidad
/// desde el exterior.
/// </summary>
public abstract class AggregateRoot(Guid id) : EntityBase(id), EntityBase
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Add(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    protected void Remove(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void Clear() => _domainEvents.Clear();
}
