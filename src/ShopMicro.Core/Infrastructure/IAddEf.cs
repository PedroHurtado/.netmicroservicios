namespace ShopMicro.Infrastructure;

using ShopMicro.Domain;

/// <summary>
/// Implementa <see cref="IAdd{T}.AddAsync"/> como método por defecto, componiendo
/// <see cref="IRepositoryEf{T, TId, TEf}"/> + <see cref="IAdd{T}"/>. La lógica se
/// escribe una sola vez y se reutiliza por composición, sin herencia de clases.
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
/// <typeparam name="TId">tipo del identificador</typeparam>
/// <typeparam name="TEf">tipo de persistencia (entidad EF)</typeparam>
public interface IAddEf<T, TId, TEf> : IRepositoryEf<T, TId, TEf>, IAdd<T>
    where T : EntityBase<TId>
    where TEf : class
{
    async Task IAdd<T>.AddAsync(T entity)
    {
        await Set().AddAsync(Mapper().ToEf(entity));
        await Db().SaveChangesAsync();
    }
}
