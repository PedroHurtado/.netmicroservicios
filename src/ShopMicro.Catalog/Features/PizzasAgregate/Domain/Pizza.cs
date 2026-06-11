namespace ShopMicro.Catalog.Features.PizzasAgregate.Domain;

using ShopMicro.Catalog.Features.IngredientsAgregate.Domain;
using ShopMicro.Domain;

/// <summary>
/// Pizza del catálogo. Raíz de agregado: referencia a un conjunto de <see cref="Ingredient"/>
/// ya resueltos (entidades de dominio reales, no ids) y <b>acumula eventos de dominio</b>.
/// Constructor y atributos protected; la fábrica <see cref="Create"/> es el único punto de
/// creación. La colección de ingredientes se expone como solo lectura para garantizar su
/// inmutabilidad desde el exterior.
/// </summary>
public class Pizza : AggregateRoot<Guid>
{
    private readonly HashSet<Ingredient> _ingredients;

    public string Name { get; protected set; }

    public string Description { get; protected set; }

    public string Url { get; protected set; }

    public IReadOnlySet<Ingredient> Ingredients => _ingredients;

    protected Pizza(Guid id, string name, string description, string url, ISet<Ingredient> ingredients)
        : base(id)
    {
        Name = name;
        Description = description;
        Url = url;
        _ingredients = [.. ingredients];
    }

    public static Pizza Create(string name, string description, string url, ISet<Ingredient> ingredients)
    {
        var pizza = new Pizza(Guid.NewGuid(), name, description, url, ingredients);
        // El agregado declara el hecho; no sabe quién reaccionará ni cómo se persiste.
        pizza.AddEvent("pizza:create", pizza);
        return pizza;
    }
}
