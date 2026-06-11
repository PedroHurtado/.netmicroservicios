namespace ShopMicro.Domain;

/// <summary>
/// Contrato <b>no genérico</b> (ISP) para que la infraestructura pueda leer y limpiar los
/// eventos acumulados por un agregado sin conocer su <c>TId</c>. El despacho en
/// <c>SaveChanges</c> solo necesita estas dos operaciones: aplanar los eventos y vaciarlos.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyList<DomainEventData> DomainEvents { get; }

    void Clear();
}
