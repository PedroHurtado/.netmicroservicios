namespace ShopMicro.Infrastructure;

/// <summary>
/// Punto único para resolver FKs por tipo de agregado. Recibe por inyección de colección
/// todos los <see cref="ILookup{T}"/> registrados (vía la marcadora <see cref="ILookupMarker"/>)
/// y los indexa por el tipo <c>T</c> que resuelve cada uno, inferido del genérico sin que la
/// interfaz tenga que declararlo.
///
/// Añadir un nuevo destino de FK NO obliga a tocar esta clase: basta registrar un nuevo
/// <see cref="ILookup{T}"/> en el contenedor. Eso es Open/Closed (OCP) en la práctica.
/// </summary>
public sealed class LookupResolver
{
    private readonly Dictionary<Type, ILookupMarker> _lookups;

    // Recibe todos los lookups del contenedor vía la marcadora. Cada uno cierra
    // ILookup<T> sobre un T distinto; ese T concreto se recupera al indexar.
    public LookupResolver(IEnumerable<ILookupMarker> lookups)
        => _lookups = lookups.ToDictionary(TargetTypeOf, lookup => lookup);

    /// <summary>Extrae el <c>T</c> de un objeto que implementa <see cref="ILookup{T}"/>.</summary>
    private static Type TargetTypeOf(ILookupMarker lookup)
    {
        var iface = lookup.GetType().GetInterfaces()
            .First(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(ILookup<>));

        return iface.GetGenericArguments()[0];   // el T de ILookup<T>
    }

    /// <summary>Resuelve una referencia. Lanza si el id no existe (propaga de <c>FindAsync</c>).</summary>
    public Task<T> FindAsync<T>(Guid id)
    {
        if (!_lookups.TryGetValue(typeof(T), out var lookup))
        {
            throw new NoLookupRegisteredException(typeof(T));
        }

        return ((ILookup<T>)lookup).FindAsync(id);   // cast encapsulado AQUÍ, no en el llamante
    }

    /// <summary>
    /// Resuelve una colección de referencias. Si algún id no existe, <c>FindAsync</c> lanza
    /// <see cref="ShopMicro.Domain.EntityNotFoundException"/> → la operación entera falla
    /// (no devuelve resultados parciales).
    /// </summary>
    public async Task<ISet<T>> FindAllAsync<T>(IEnumerable<Guid> ids)
    {
        var result = new HashSet<T>();
        foreach (var id in ids)
        {
            result.Add(await FindAsync<T>(id));
        }

        return result;
    }
}
