namespace ShopMicro.Infrastructure;

using Microsoft.EntityFrameworkCore;
using ShopMicro.Domain;

/// <summary>
/// Base "sin herencia" de los repositorios EF. Aísla los únicos puntos variables
/// que cada repositorio concreto debe cablear: el <see cref="DbContext"/> (unidad de
/// trabajo), el <see cref="DbSet{TEf}"/> (la "tabla") y el mapper dominio ↔ EF.
/// </summary>
/// <typeparam name="T">tipo de dominio (extiende <see cref="EntityBase{TId}"/> para extraer el id)</typeparam>
/// <typeparam name="TId">tipo del identificador</typeparam>
/// <typeparam name="TEf">tipo de persistencia (entidad EF)</typeparam>
public interface IRepositoryEf<T, TId, TEf>
    where T : EntityBase<TId>
    where TEf : class
{
    DbContext Db();              // unidad de trabajo (SaveChangesAsync)

    DbSet<TEf> Set();            // la "tabla"

    IMapper<T, TEf> Mapper();    // dominio ↔ EF
}
