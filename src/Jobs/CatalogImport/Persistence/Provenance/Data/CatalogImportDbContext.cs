using Microsoft.EntityFrameworkCore;

namespace CatalogImport.Persistence;

public sealed class CatalogImportDbContext(DbContextOptions<CatalogImportDbContext> options) : DbContext(options)
{
    public DbSet<CatalogImportBatch> Batches => Set<CatalogImportBatch>();
    public DbSet<SourceIdMapping> Mappings => Set<SourceIdMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CatalogImportBatchConfiguration());
        modelBuilder.ApplyConfiguration(new SourceIdMappingConfiguration());
    }
}

