namespace ShopMicro.WebApi;

/// <summary>
/// Marca un slice (vertical slice) como módulo de feature. Cada implementación recibe
/// por constructor un <see cref="Microsoft.AspNetCore.Routing.IEndpointRouteBuilder"/>
/// y mapea en él sus endpoints.
///
/// La extensión <c>MapFeatures()</c> descubre por reflexión todas las implementaciones
/// de esta interfaz y las instancia pasándoles el route builder, de modo que cada
/// feature registra sus propias rutas sin que el host las conozca explícitamente.
/// </summary>
public interface IFeatureModule
{
}
