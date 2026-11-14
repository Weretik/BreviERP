using Catalog.Domain.ProductCategories;
using Catalog.Domain.Products;

namespace Catalog.Infrastructure.DataBase;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly,
            type => type.Namespace?.StartsWith("Catalog.Infrastructure") ?? false);

    }
}
