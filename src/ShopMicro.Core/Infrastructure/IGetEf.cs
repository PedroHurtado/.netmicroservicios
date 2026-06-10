namespace ShopMicro.Infrastructure;

using ShopMicro.Domain;

/// <summary>
/// Implementa <see cref="IGet{T, TId}.GetAsync"/> como método por defecto: busca por
/// id, mapea a dominio o lanza <see cref="EntityNotFoundException"/> si no existe.
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
/// <typeparam name="TId">tipo del identificador</typeparam>
/// <typeparam name="TEf">tipo de persistencia (entidad EF)</typeparam>
public interface IGetEf<T, TId, TEf> : IRepositoryEf<T, TId, TEf>, IGet<T, TId>
    where T : EntityBase<TId>
    where TEf : class
{
    /// <summary>Tipo de dominio, usado para construir la excepción cuando no se encuentra.</summary>
    Type DomainType() => typeof(T);

    async Task<T> IGet<T, TId>.GetAsync(TId id)
    {
        var ef = await Set().FindAsync(id);
        if (ef is null)
        {
            throw new EntityNotFoundException(DomainType(), id!);
        }

        return Mapper().ToDomain(ef);
    }
}
