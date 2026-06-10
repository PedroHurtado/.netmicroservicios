namespace ShopMicro.Infrastructure;

using ShopMicro.Domain;

/// <summary>
/// Acción de leer por id. <c>GetAsync</c> devuelve <typeparamref name="T"/> o lanza
/// <see cref="EntityNotFoundException"/>; nunca devuelve null ni un tipo opcional.
/// Es la operación base de la que dependen borrar y actualizar.
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
/// <typeparam name="TId">tipo del identificador</typeparam>
public interface IGet<T, in TId>
{
    Task<T> GetAsync(TId id);
}
