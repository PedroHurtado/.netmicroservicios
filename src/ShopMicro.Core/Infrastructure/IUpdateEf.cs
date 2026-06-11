namespace ShopMicro.Infrastructure;

using ShopMicro.Domain;

/// <summary>
/// Implementa <see cref="IUpdate{T, TId}.UpdateAsync"/> como método por defecto.
/// NO vuelve a leer: el handler ya leyó la entidad con <c>GetAsync</c> para aplicarle los
/// cambios de dominio, y esa lectura ya garantizó la existencia (lanza 404 si no). Aquí solo
/// se mapea a EF y se persiste, de modo que el caso de uso hace <b>una sola lectura</b>.
/// Extiende <see cref="IGetEf{T, TId, TEf}"/> para que la vista de "actualizar" ofrezca al
/// handler tanto <c>GetAsync</c> (leer) como <c>UpdateAsync</c> (guardar) en una sola interfaz.
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
/// <typeparam name="TId">tipo del identificador</typeparam>
/// <typeparam name="TEf">tipo de persistencia (entidad EF)</typeparam>
public interface IUpdateEf<T, TId, TEf> : IGetEf<T, TId, TEf>, IUpdate<T, TId>
    where T : EntityBase<TId>
    where TEf : class
{
    async Task IUpdate<T, TId>.UpdateAsync(T entity)
    {
        // La lectura ya la hizo el handler; aquí solo se persiste sobre una entidad EF nueva.
        Set().Update(Mapper().ToEf(entity));
        await Db().SaveChangesAsync();
    }
}
