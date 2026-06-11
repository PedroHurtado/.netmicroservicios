namespace ShopMicro.Catalog.Features.PizzasAgregate.Persistence;

using ShopMicro.Catalog.Features.PizzasAgregate.Domain;
using ShopMicro.Infrastructure;

/// <summary>
/// Mapper manual Pizza ↔ PizzaEf. <c>ToEf</c> proyecta los ingredientes a sus ids (Pizza
/// referencia a Ingredient por identidad). <c>ToDomain</c> queda fuera del alcance de esta
/// lección: rehidratar una Pizza completa exige resolver esos ids → entidades Ingredient,
/// lo que corresponde a la slice de lectura (GetPizza) usando el <c>LookupResolver</c>, no a
/// un mapper. El repositorio de Pizza solo implementa Add, así que ToDomain nunca se invoca.
/// </summary>
public sealed class PizzaMapper : IMapper<Pizza, PizzaEf>
{
    public PizzaEf ToEf(Pizza domain)
        => new(
            domain.Id,
            domain.Name,
            domain.Description,
            domain.Url,
            domain.Ingredients.Select(ingredient => ingredient.Id).ToList());

    public Pizza ToDomain(PizzaEf ef)
        => throw new NotSupportedException(
            "Rehidratar Pizza requiere resolver los ids de ingrediente (LookupResolver); " +
            "corresponde a la slice GetPizza, fuera del alcance de 6.10.");
}
