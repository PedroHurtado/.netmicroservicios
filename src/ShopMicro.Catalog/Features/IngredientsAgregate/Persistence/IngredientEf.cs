namespace ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;

/// <summary>
/// Entidad EF de Ingredient: modela la <b>fila</b> de la tabla, no el dominio.
/// Es un simple contenedor de datos (setters, ctor sin lógica), separado del agregado
/// para mantener el dominio 100% libre de EF. Es lo que expone el <c>DbSet</c>.
/// </summary>
public class IngredientEf
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public decimal Cost { get; set; }

    // Constructor sin parámetros requerido por EF Core.
    public IngredientEf()
    {
    }

    public IngredientEf(Guid id, string name, decimal cost)
    {
        Id = id;
        Name = name;
        Cost = cost;
    }
}
