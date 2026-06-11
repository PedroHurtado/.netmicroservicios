namespace ShopMicro.Catalog.Features.PizzasAgregate.Persistence;

/// <summary>
/// Entidad EF de Pizza: modela la <b>fila</b>, no el dominio. Referencia a sus ingredientes
/// <b>por id</b> (colección primitiva de <see cref="Guid"/>), no por navegación a la entidad
/// EF de Ingredient: Pizza e Ingredient son agregados distintos y un agregado referencia a
/// otro por identidad (DDD). Así, persistir una pizza NO toca la tabla de ingredientes, y el
/// <c>AddAsync</c> genérico del núcleo funciona sin reescribirse.
/// </summary>
public class PizzaEf
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Url { get; set; } = default!;

    public List<Guid> IngredientIds { get; set; } = [];

    // Constructor sin parámetros requerido por EF Core.
    public PizzaEf()
    {
    }

    public PizzaEf(Guid id, string name, string description, string url, List<Guid> ingredientIds)
    {
        Id = id;
        Name = name;
        Description = description;
        Url = url;
        IngredientIds = ingredientIds;
    }
}
