namespace ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;

using ShopMicro.Catalog.Features.IngredientsAgregate.Domain;
using ShopMicro.Infrastructure;

/// <summary>
/// Lookup de <see cref="Ingredient"/>: lo registra porque es destino de FK desde Pizza.
/// Se apoya en <see cref="IGetIngredient"/> (puerto de lectura del núcleo, 6.3), que ya
/// lanza <see cref="ShopMicro.Domain.EntityNotFoundException"/> si el id no existe; al ser
/// async de extremo a extremo, simplemente reenvía <c>GetAsync</c> sin adaptadores.
/// </summary>
public sealed class IngredientLookup(IGetIngredient get) : ILookup<Ingredient>
{
    public Task<Ingredient> FindAsync(Guid id) => get.GetAsync(id);
}
