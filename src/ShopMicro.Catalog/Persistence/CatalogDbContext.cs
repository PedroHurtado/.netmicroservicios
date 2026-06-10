namespace ShopMicro.Catalog.Persistence;

using Microsoft.EntityFrameworkCore;
using ShopMicro.Catalog.Features.IngredientsAgregate.Persistence;

/// <summary>
/// DbContext del microservicio Catalog. Persiste las <b>entidades EF</b> (la fila),
/// nunca el dominio: por eso los <c>DbSet</c> exponen los tipos <c>*Ef</c>.
/// La traducción dominio ↔ EF la hacen los mappers de cada slice.
/// </summary>
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<IngredientEf> Ingredients => Set<IngredientEf>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IngredientEf>(entity =>
        {            
            entity.HasKey(ingredient => ingredient.Id);
            entity.Property(ingredient => ingredient.Name).IsRequired().HasMaxLength(200);
        });
    }
}
