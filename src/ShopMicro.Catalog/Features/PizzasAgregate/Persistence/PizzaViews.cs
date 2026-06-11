namespace ShopMicro.Catalog.Features.PizzasAgregate.Persistence;

using ShopMicro.Catalog.Features.PizzasAgregate.Domain;
using ShopMicro.Infrastructure;

/// <summary>
/// "Vistas" del repositorio de Pizza: fijan los genéricos del núcleo para este agregado y
/// son la superficie pública hacia la aplicación. Solo hablan de dominio, no de EF. Cada
/// handler inyecta únicamente la vista que necesita (ISP).
/// </summary>

/// <summary>Vista de "añadir pizza".</summary>
public interface IAddPizza : IAdd<Pizza>;
