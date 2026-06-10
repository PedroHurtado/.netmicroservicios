namespace ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;

using ShopMicro.Catalog.Features.IngredientsAgregate.Domain;
using ShopMicro.Infrastructure;

/// <summary>
/// Mapper manual Ingredient ↔ IngredientEf. Reconstruye el dominio con la subclase
/// <see cref="Hydration"/>, que invoca el constructor protected de Ingredient. Ese
/// constructor NO emite eventos, de modo que reconstruir desde BD nunca dispara eventos
/// de dominio aunque Ingredient pase a ser un agregado que los emita en su fábrica.
/// </summary>
public sealed class IngredientMapper : IMapper<Ingredient, IngredientEf>
{
    public IngredientEf ToEf(Ingredient domain)
        => new(domain.Id, domain.Name, domain.Cost);

    public Ingredient ToDomain(IngredientEf ef)
        => new Hydration(ef.Id, ef.Name, ef.Cost);

    /// <summary>Subclase de hidratación: invoca el constructor protected → sin eventos.</summary>
    private sealed class Hydration(Guid id, string name, decimal cost) : Ingredient(id, name, cost);
}
