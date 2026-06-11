namespace ShopMicro.Infrastructure;

using Microsoft.EntityFrameworkCore;
using ShopMicro.Domain;

/// <summary>
/// Implementa <see cref="IGet{T, TId}.GetAsync"/> como método por defecto: busca por
/// id, mapea a dominio o lanza <see cref="EntityNotFoundException"/> si no existe.
///
/// La lectura es <b>sin tracking</b>: <c>GetAsync</c> solo devuelve una instantánea de
/// solo lectura. Las escrituras (<see cref="IAddEf{T, TId, TEf}"/>,
/// <see cref="IUpdateEf{T, TId, TEf}"/>) construyen una entidad EF <b>nueva</b> con el
/// mapper y la (re)adjuntan; si <c>GetAsync</c> dejara la fila rastreada, ese <c>Update</c>
/// chocaría con la instancia ya rastreada (misma clave). Por eso leer nunca rastrea.
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
        // AsNoTracking + clave por convención ("Id"): lectura genérica por PK sin rastrear.
        var ef = await Set().AsNoTracking()
            .FirstOrDefaultAsync(entity => EF.Property<TId>(entity, "Id")!.Equals(id));
        if (ef is null)
        {
            throw new EntityNotFoundException(DomainType(), id!);
        }

        return Mapper().ToDomain(ef);
    }
}
