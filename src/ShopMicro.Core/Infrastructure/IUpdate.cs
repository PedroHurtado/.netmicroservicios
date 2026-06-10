namespace ShopMicro.Infrastructure;

/// <summary>
/// Acción de actualizar. Extiende <see cref="IGet{T, TId}"/>: para actualizar,
/// primero hay que leer (garantizar que la entidad existe).
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
/// <typeparam name="TId">tipo del identificador</typeparam>
public interface IUpdate<T, TId> : IGet<T, TId>
{
    Task UpdateAsync(T entity);
}
