using Domain.Core;

namespace Domain;

/// <summary>
/// Pizza. Raíz de agregado.
/// La relación con los ingredientes es un Set (no admite duplicados; la
/// igualdad de <see cref="Ingredient"/> es por identificador) y se expone
/// como colección inmutable.
/// El precio no se persiste: es un getter calculado a partir del coste de
/// los ingredientes más un 20 % de beneficio.
/// Constructor y atributos protected.
/// </summary>
public class Pizza : AggregateRoot
{
    private const decimal ProfitMargin = 1.20m;

    private readonly HashSet<Ingredient> _ingredients;

    public string Name { get; protected set; }

    public string Description { get; protected set; }

    public string Url { get; protected set; }

    public decimal Price => _ingredients.Sum(ingredient => ingredient.Cost) * ProfitMargin;

    public IReadOnlySet<Ingredient> Ingredients => _ingredients;

    protected Pizza(
        Guid id,
        string name,
        string description,
        string url,
        IEnumerable<Ingredient> ingredients)
        : base(id)
    {
        Name = name;
        Description = description;
        Url = url;
        _ingredients = [.. ingredients];
    }

    public static Pizza Create(
        Guid id,
        string name,
        string description,
        string url,
        IEnumerable<Ingredient> ingredients)
        => new(id, name, description, url, ingredients);

    public void Update(string name, string description, string url)
    {
        Name = name;
        Description = description;
        Url = url;
    }

    public void AddIngredient(Ingredient ingredient) => _ingredients.Add(ingredient);

    public void RemoveIngredient(Ingredient ingredient) => _ingredients.Remove(ingredient);
}
