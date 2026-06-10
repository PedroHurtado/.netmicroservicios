namespace ShopMicro.Catalog.Features.IngredientsAgregate.Commands;

using MediatR;
using ShopMicro.Catalog.Features.IngredientsAgregate.Domain;
using ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;
using ShopMicro.WebApi;

/// <summary>
/// Feature "crear ingrediente" (vertical slice). Como <see cref="IFeatureModule"/>,
/// recibe por constructor el route builder y mapea en él su endpoint. El endpoint solo
/// despacha el <see cref="Command"/> con MediatR; la coordinación vive en <see cref="Handler"/>.
/// </summary>
public class IngredienteCreate : IFeatureModule
{
    /// <summary>Comando de entrada del caso de uso.</summary>
    public record Command(string Name, decimal Cost) : IRequest<Response>;

    /// <summary>Respuesta del caso de uso.</summary>
    public record Response(Guid Id, string Name, decimal Cost);

    public IngredienteCreate(IEndpointRouteBuilder routes)
    {
        routes.MapPost("/ingredients", async (Command command, ISender sender) =>
        {
            var response = await sender.Send(command);
            return Results.Created($"/ingredients/{response.Id}", response);
        })
        .WithName("CreateIngredient");
    }

    /// <summary>
    /// Coordinador del caso de uso. MediatR lo resuelve e invoca <see cref="Handle"/> al
    /// despachar el <see cref="Command"/>. No requiere interfaz propia.
    /// </summary>
    public class Handler(IAddIngredient repository) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command command, CancellationToken cancellationToken)
        {
            var ingredient = Ingredient.Create(Guid.NewGuid(), command.Name, command.Cost);

            await repository.AddAsync(ingredient);

            return new Response(ingredient.Id, ingredient.Name, ingredient.Cost);
        }
    }
}
