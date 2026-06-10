namespace ShopMicro.Infrastructure;

using ShopMicro.Domain;

/// <summary>
/// Implementa <see cref="IUpdate{T, TId}.UpdateAsync"/> como método por defecto.
/// Extiende <see cref="IGetEf{T, TId, TEf}"/> para reutilizar su <c>GetAsync</c>
/// (garantiza la existencia antes de guardar).
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
        await GetAsync(entity.Id);           // lanza EntityNotFoundException si no existe
        Set().Update(Mapper().ToEf(entity));
        await Db().SaveChangesAsync();
    }
}
