using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox;
using BuildingBlocks.Infrastructure.Database;
using Catalog.Domain.ProductCategories;
using Catalog.Domain.Products;

namespace Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddTrigramSearch();
        // The catalogue names its schema per table, so its outbox does too.
        modelBuilder.AddOutbox().Entity<OutboxMessage>().ToTable(OutboxModelBuilderExtensions.Table, "catalog");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly,
            type => type.Namespace?.StartsWith("Catalog.Infrastructure") ?? false);

    }
}
