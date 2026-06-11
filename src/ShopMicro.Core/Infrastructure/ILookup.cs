namespace ShopMicro.Infrastructure;

/// <summary>
/// Interfaz marcadora (vacía). No aporta métodos: existe solo para que el contenedor de
/// dependencias pueda recoger TODOS los lookups con una sola consulta
/// (<c>GetServices&lt;ILookupMarker&gt;()</c>) sin tener que registrar cada uno como <c>object</c>.
/// </summary>
public interface ILookupMarker
{
}

/// <summary>
/// Resuelve una referencia (FK) a un agregado de dominio por id. Cada agregado que sea
/// destino de FK registra su propia implementación de <see cref="ILookup{T}"/>.
///
/// <c>FindAsync</c> devuelve <typeparamref name="T"/> o lanza
/// <see cref="ShopMicro.Domain.EntityNotFoundException"/> si no existe — misma semántica que
/// <see cref="IGet{T, TId}"/>: nunca null, nunca opcional. El tipo que resuelve NO se declara
/// aquí: lo infiere el <see cref="LookupResolver"/> del propio genérico al construir su índice.
/// </summary>
/// <typeparam name="T">tipo de dominio destino de la referencia</typeparam>
public interface ILookup<T> : ILookupMarker
{
    Task<T> FindAsync(Guid id);
}
