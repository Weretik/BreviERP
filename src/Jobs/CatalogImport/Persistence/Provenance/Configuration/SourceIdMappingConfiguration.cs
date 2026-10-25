using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogImport.Persistence;

public sealed class SourceIdMappingConfiguration : IEntityTypeConfiguration<SourceIdMapping>
{
    public void Configure(EntityTypeBuilder<SourceIdMapping> builder)
    {
        builder.ToTable("CatalogSourceMappings", "imports");
        builder.HasKey(x => new { x.Dataset, x.SourceKey });
        builder.Property(x => x.Dataset).HasMaxLength(100);
        builder.Property(x => x.SourceKey).HasMaxLength(200);
        builder.Property(x => x.TargetType).HasMaxLength(100);
        builder.HasOne<CatalogImportBatch>().WithMany().HasForeignKey(x => x.SnapshotHash);
    }
}
