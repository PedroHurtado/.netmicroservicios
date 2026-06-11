namespace ShopMicro.Catalog.Features.PizzasAgregate.Commands;

using MediatR;
using ShopMicro.Catalog.Features.IngredientsAgregate.Domain;
using ShopMicro.Catalog.Features.PizzasAgregate.Domain;
using ShopMicro.Catalog.Features.PizzasAgregate.Persistence;
using ShopMicro.Infrastructure;
using ShopMicro.WebApi;

/// <summary>
/// Feature "crear pizza" (vertical slice). El comando trae ids de ingredientes, no entidades:
/// el <see cref="Handler"/> los resuelve con el <see cref="LookupResolver"/> — NO inyecta el
/// <c>IngredientLookup</c> concreto. Si mañana la pizza referencia otro agregado, bastará
/// registrar su <c>ILookup&lt;T&gt;</c>; ni el resolver ni este handler cambian (OCP).
/// </summary>
public class PizzaCreate : IFeatureModule
{
    /// <summary>Comando de entrada: la pizza llega con ids de ingredientes.</summary>
    public record Command(string Name, string Description, string Url, IReadOnlyList<Guid> IngredientIds)
        : IRequest<Response>;

    /// <summary>Respuesta del caso de uso.</summary>
    public record Response(Guid Id);

    public PizzaCreate(IEndpointRouteBuilder routes)
    {
        routes.MapPost("/pizzas", async (Command command, ISender sender) =>
        {
            var response = await sender.Send(command);
            return Results.Created($"/pizzas/{response.Id}", response);
        });
    }

    /// <summary>
    /// Coordinador del caso de uso. Inyecta el <see cref="LookupResolver"/> (no un lookup
    /// concreto) y la vista <see cref="IAddPizza"/>.
    /// </summary>
    public class Handler(IAddPizza repository, LookupResolver resolver) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command command, CancellationToken cancellationToken)
        {
            // ids → entidades de dominio (lanza EntityNotFoundException si algún ingrediente no existe).
            ISet<Ingredient> ingredients = await resolver.FindAllAsync<Ingredient>(command.IngredientIds);

            var pizza = Pizza.Create(command.Name, command.Description, command.Url, ingredients);

            await repository.AddAsync(pizza);

            return new Response(pizza.Id);
        }
    }
}
