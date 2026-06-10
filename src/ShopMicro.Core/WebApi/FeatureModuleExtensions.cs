namespace ShopMicro.WebApi;

using System.Reflection;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Extensiones para auto-registrar los <see cref="IFeatureModule"/> de una aplicación.
/// </summary>
public static class FeatureModuleExtensions
{
    /// <summary>
    /// Descubre todas las implementaciones de <see cref="IFeatureModule"/> en los
    /// ensamblados indicados (o, por defecto, en el ensamblado que llama) y las instancia
    /// pasándoles el <paramref name="routes"/>. Al construirse, cada módulo mapea en él
    /// sus propios endpoints.
    /// </summary>
    /// <param name="routes">constructor de rutas sobre el que cada feature mapea sus endpoints</param>
    /// <param name="assemblies">ensamblados a escanear; si se omiten, se usa el ensamblado llamante</param>
    public static IEndpointRouteBuilder MapFeatures(
        this IEndpointRouteBuilder routes,
        params Assembly[] assemblies)
    {
        var scan = assemblies.Length > 0 ? assemblies : [Assembly.GetCallingAssembly()];

        var moduleTypes = scan
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && typeof(IFeatureModule).IsAssignableFrom(type));

        foreach (var type in moduleTypes)
        {
            Activator.CreateInstance(type, routes);
        }

        return routes;
    }
}
