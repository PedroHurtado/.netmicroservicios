namespace ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;

using Microsoft.EntityFrameworkCore;
using ShopMicro.Catalog.Features.IngredientsAgregate.Domain;
using ShopMicro.Catalog.Persistence;
using ShopMicro.Infrastructure;

/// <summary>
/// Repositorio de Ingredient. Compone las vistas + las interfaces <c>*Ef</c> del núcleo
/// y solo cablea los tres puntos variables (<c>Db()</c>, <c>Set()</c>, <c>Mapper()</c>).
/// add/get/update/remove provienen de los default interface methods del núcleo: al ser
/// Ingredient un agregado plano (sin navegaciones), <c>GetAsync</c> por id basta y no se
/// sobrescribe (a diferencia de Pizza, que necesita <c>Include</c>).
/// </summary>
public sealed class IngredientRepository(CatalogDbContext db, IngredientMapper mapper) :
    IAddIngredient,
    IGetIngredient,
    IUpdateIngredient,
    IRemoveIngredient,
    IAddEf<Ingredient, Guid, IngredientEf>,
    IGetEf<Ingredient, Guid, IngredientEf>,
    IUpdateEf<Ingredient, Guid, IngredientEf>,
    IRemoveEf<Ingredient, Guid, IngredientEf>
{
    public DbContext Db() => db;

    public DbSet<IngredientEf> Set() => db.Ingredients;

    public IMapper<Ingredient, IngredientEf> Mapper() => mapper;
}
