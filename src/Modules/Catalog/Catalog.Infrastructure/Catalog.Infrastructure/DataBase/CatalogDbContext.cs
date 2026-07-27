using Catalog.Domain.Media.Entities;
using Catalog.Domain.ProductCategories.Entities;
using Catalog.Domain.Products.Entities;

namespace Catalog.Infrastructure.DataBase;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
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