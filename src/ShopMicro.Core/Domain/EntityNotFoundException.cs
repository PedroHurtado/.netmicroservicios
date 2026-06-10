namespace ShopMicro.Domain;

/// <summary>
/// Se lanza cuando una entidad no existe. La capa de errores (web) la traduce a un
/// <c>ProblemDetails</c> con <c>status: 404</c>: la capa de datos <b>lanza</b>, la
/// capa web <b>traduce</b>.
/// </summary>
public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(Type domainType, object id)
        : base($"{domainType.Name} con id '{id}' no encontrado.")
    {
    }
}
