using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopMicro.Catalog.Features.IngredientsAgregate.Commands;
using ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;
using ShopMicro.Catalog.Features.PizzasAgregate.Persistence;
using ShopMicro.Catalog.Persistence;
using ShopMicro.Infrastructure;
using ShopMicro.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// MediatR: registra los Handler de los slices de este ensamblado.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IngredienteCreate>());

// Persistencia: DbContext InMemory (persiste las entidades EF, no el dominio).
builder.Services.AddDbContext<CatalogDbContext>(options => options.UseInMemoryDatabase("catalog"));

// Mapper y repositorio de Ingredient, expuesto por cada vista (ISP): cada handler
// inyecta solo la que necesita.
builder.Services.AddScoped<IngredientMapper>();
builder.Services.AddScoped<IngredientRepository>();
builder.Services.AddScoped<IAddIngredient>(sp => sp.GetRequiredService<IngredientRepository>());
builder.Services.AddScoped<IGetIngredient>(sp => sp.GetRequiredService<IngredientRepository>());
builder.Services.AddScoped<IUpdateIngredient>(sp => sp.GetRequiredService<IngredientRepository>());
builder.Services.AddScoped<IRemoveIngredient>(sp => sp.GetRequiredService<IngredientRepository>());

// Lookup / LookupResolver (OCP): cada agregado destino de FK registra su ILookup vía la
// marcadora ILookupMarker. El resolver los recibe TODOS con una sola consulta al contenedor;
// añadir un destino nuevo es una línea de registro más, sin tocar el resolver.
builder.Services.AddScoped<ILookupMarker, IngredientLookup>();
builder.Services.AddScoped<LookupResolver>(sp =>
    new LookupResolver(sp.GetServices<ILookupMarker>()));

// Mapper y repositorio de Pizza, expuesto por su vista (ISP).
builder.Services.AddScoped<PizzaMapper>();
builder.Services.AddScoped<PizzaRepository>();
builder.Services.AddScoped<IAddPizza>(sp => sp.GetRequiredService<PizzaRepository>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Descubre e instancia todos los IFeatureModule del ensamblado, mapeando sus endpoints.
app.MapFeatures();

app.Run();
