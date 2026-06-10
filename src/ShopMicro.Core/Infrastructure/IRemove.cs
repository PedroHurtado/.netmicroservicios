namespace ShopMicro.Infrastructure;

/// <summary>
/// Acción de borrar. Extiende <see cref="IGet{T, TId}"/>: para borrar, primero hay
/// que garantizar que la entidad existe. La herencia expresa esa precondición en el
/// sistema de tipos.
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
/// <typeparam name="TId">tipo del identificador</typeparam>
public interface IRemove<T, TId> : IGet<T, TId>
{
    Task RemoveAsync(TId id);
}
