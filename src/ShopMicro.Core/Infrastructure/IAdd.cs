namespace ShopMicro.Infrastructure;

/// <summary>
/// Acción de añadir (ISP: una interfaz por operación). Añadir una entidad nueva
/// no requiere leer nada, por eso NO extiende <see cref="IGet{T, TId}"/>.
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
public interface IAdd<in T>
{
    Task AddAsync(T entity);
}
