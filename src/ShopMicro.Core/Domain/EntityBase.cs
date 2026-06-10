namespace ShopMicro.Domain;

/// <summary>
/// Raíz de todas las entidades del dominio. Expone el identificador (de tipo
/// genérico <typeparamref name="TId"/>) que necesitan las operaciones genéricas
/// de la capa de persistencia.
/// Dos entidades son iguales cuando comparten tipo e identificador.
/// El identificador se fija en construcción/hidratación y solo se expone vía
/// getter (con <c>protected set</c>), lo que garantiza su inmutabilidad externa.
/// </summary>
/// <typeparam name="TId">tipo del identificador (p. ej. <see cref="Guid"/>)</typeparam>
public abstract class EntityBase<TId>
{
    public TId Id { get; protected set; } = default!;

    protected EntityBase()
    {
    }

    protected EntityBase(TId id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase<TId> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => Id is null ? 0 : Id.GetHashCode();

    public static bool operator ==(EntityBase<TId>? left, EntityBase<TId>? right) => Equals(left, right);

    public static bool operator !=(EntityBase<TId>? left, EntityBase<TId>? right) => !Equals(left, right);
}
