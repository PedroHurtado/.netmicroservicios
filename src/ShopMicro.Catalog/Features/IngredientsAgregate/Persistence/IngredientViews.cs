namespace ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;

using ShopMicro.Catalog.Features.IngredientsAgregate.Domain;
using ShopMicro.Infrastructure;

/// <summary>
/// "Vistas" del repositorio de Ingredient: fijan los genéricos del núcleo para este
/// agregado y son la superficie pública hacia la aplicación. Solo hablan de dominio,
/// no de EF. Cada handler inyecta únicamente la vista que necesita (ISP).
/// </summary>

/// <summary>Vista de "añadir ingrediente".</summary>
public interface IAddIngredient : IAdd<Ingredient>;

/// <summary>Vista de "leer ingrediente por id". GetAsync lanza si no existe.</summary>
public interface IGetIngredient : IGet<Ingredient, Guid>;

/// <summary>Vista de "actualizar ingrediente". Hereda GetAsync de IUpdate → IGet.</summary>
public interface IUpdateIngredient : IUpdate<Ingredient, Guid>;

/// <summary>Vista de "borrar ingrediente". Hereda GetAsync de IRemove → IGet.</summary>
public interface IRemoveIngredient : IRemove<Ingredient, Guid>;
