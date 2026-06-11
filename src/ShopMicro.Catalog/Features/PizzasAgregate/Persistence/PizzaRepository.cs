namespace ShopMicro.Catalog.Features.PizzasAgregate.Persistence;

using Microsoft.EntityFrameworkCore;
using ShopMicro.Catalog.Features.PizzasAgregate.Domain;
using ShopMicro.Catalog.Persistence;
using ShopMicro.Infrastructure;

/// <summary>
/// Repositorio de Pizza. Como Pizza referencia a sus ingredientes por id (no por navegación),
/// persistirla solo toca su propia tabla: reutiliza el <c>AddAsync</c> por defecto del núcleo
/// sin reescribir nada, igual que Ingredient. Solo cablea los tres puntos variables
/// (<c>Db()</c>, <c>Set()</c>, <c>Mapper()</c>).
/// </summary>
public sealed class PizzaRepository(CatalogDbContext db, PizzaMapper mapper) :
    IAddPizza,
    IAddEf<Pizza, Guid, PizzaEf>
{
    public DbContext Db() => db;

    public DbSet<PizzaEf> Set() => db.Pizzas;

    public IMapper<Pizza, PizzaEf> Mapper() => mapper;
}
