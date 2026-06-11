namespace ShopMicro.Catalog.Persistence;

using Microsoft.EntityFrameworkCore;
using ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;
using ShopMicro.Catalog.Features.PizzasAgregate.Persistence;

/// <summary>
/// DbContext del microservicio Catalog. Persiste las <b>entidades EF</b> (la fila),
/// nunca el dominio: por eso los <c>DbSet</c> exponen los tipos <c>*Ef</c>.
/// La traducción dominio ↔ EF la hacen los mappers de cada slice.
/// </summary>
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<IngredientEf> Ingredients => Set<IngredientEf>();

    public DbSet<PizzaEf> Pizzas => Set<PizzaEf>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IngredientEf>(entity =>
        {
            entity.HasKey(ingredient => ingredient.Id);
            entity.Property(ingredient => ingredient.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<PizzaEf>(entity =>
        {
            entity.HasKey(pizza => pizza.Id);
            entity.Property(pizza => pizza.Name).IsRequired().HasMaxLength(200);
            // IngredientIds es una colección primitiva (ids de otro agregado): sin navegación
            // ni tabla de unión hacia IngredientEf.
        });
    }
}
