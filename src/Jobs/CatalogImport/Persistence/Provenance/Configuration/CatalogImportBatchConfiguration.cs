using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogImport.Persistence;

public sealed class CatalogImportBatchConfiguration : IEntityTypeConfiguration<CatalogImportBatch>
{
    public void Configure(EntityTypeBuilder<CatalogImportBatch> builder)
    {
        builder.ToTable("CatalogImportBatches", "imports");
        builder.HasKey(x => x.SnapshotHash);
        builder.Property(x => x.SnapshotHash).HasMaxLength(64);
        builder.Property(x => x.SpreadsheetId).HasMaxLength(200);
        builder.Property(x => x.SourceJson).HasColumnType("jsonb");
        builder.Property(x => x.ReportJson).HasColumnType("jsonb");
    }
}
