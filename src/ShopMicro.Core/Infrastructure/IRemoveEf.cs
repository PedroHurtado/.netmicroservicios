namespace ShopMicro.Infrastructure;

using ShopMicro.Domain;

/// <summary>
/// Implementa <see cref="IRemove{T, TId}.RemoveAsync"/> como método por defecto. Borrar no
/// necesita el objeto de dominio (ni mapear nada): localiza la fila EF por id con
/// <b>una sola lectura</b>, lanza <see cref="EntityNotFoundException"/> si no existe y la elimina.
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
        // FindAsync (con tracking) localiza la fila a eliminar: única lectura del borrado.
        var ef = await Set().FindAsync(id);
        if (ef is null)
        {
            throw new EntityNotFoundException(DomainType(), id!);
        }

        Set().Remove(ef);
        await Db().SaveChangesAsync();
    }
}
