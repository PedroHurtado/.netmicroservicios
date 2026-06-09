using Domain.Core;

namespace Domain;

/// <summary>
/// Ingrediente de una pizza. Entidad con identidad propia.
/// Constructor y atributos protected; mutación controlada vía Update.
/// </summary>
public class Ingredient : EntityBase
{
    public string Name { get; protected set; }

    public decimal Cost { get; protected set; }

    protected Ingredient(Guid id, string name, decimal cost)
        : base(id)
    {
        Name = name;
        Cost = cost;
    }

    public static Ingredient Create(Guid id, string name, decimal cost)
        => new(id, name, cost);

    public void Update(string name, decimal cost)
    {
        Name = name;
        Cost = cost;
    }
}
