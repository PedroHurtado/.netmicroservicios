namespace ShopMicro.Infrastructure;

using ShopMicro.Domain;

/// <summary>
/// Implementa <see cref="IRemove{T, TId}.RemoveAsync"/> como método por defecto.
/// Extiende <see cref="IGetEf{T, TId, TEf}"/> para garantizar la existencia (y lanzar
/// <see cref="EntityNotFoundException"/> si no) antes del borrado.
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
/// <typeparam name="TId">tipo del identificador</typeparam>
/// <typeparam name="TEf">tipo de persistencia (entidad EF)</typeparam>
public interface IRemoveEf<T, TId, TEf> : IGetEf<T, TId, TEf>, IRemove<T, TId>
    where T : EntityBase<TId>
    where TEf : class
{
    async Task IRemove<T, TId>.RemoveAsync(TId id)
    {
        await GetAsync(id);                  // lanza EntityNotFoundException si no existe
        var ef = await Set().FindAsync(id);
        Set().Remove(ef!);
        await Db().SaveChangesAsync();
    }
}
