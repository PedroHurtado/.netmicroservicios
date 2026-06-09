namespace Domain.Core;

/// <summary>
/// Base de todas las entidades del dominio.
/// Dos entidades son iguales cuando comparten tipo e identificador.
/// El identificador se fija en construcción y solo se expone vía getter,
/// lo que garantiza su inmutabilidad.
/// </summary>
public abstract class EntityBase(Guid id)
{
    public Guid Id { get; } = id;

    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase other)
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

        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(EntityBase? left, EntityBase? right) => Equals(left, right);

    public static bool operator !=(EntityBase? left, EntityBase? right) => !Equals(left, right);
}
