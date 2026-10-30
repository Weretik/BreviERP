using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence.Documents.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Production;

public sealed class ProductionPackageArtifactConfiguration : IEntityTypeConfiguration<ProductionPackageArtifact>
{
    public void Configure(EntityTypeBuilder<ProductionPackageArtifact> b)
    {
        b.ToTable("ProductionPackageArtifacts", "crm", t =>
        {
            t.HasCheckConstraint("CK_ProductionPackageArtifacts_Content", "octet_length(\"Bytes\") = \"Length\" AND \"Length\" > 0 AND \"Length\" <= 67108864");
            t.HasCheckConstraint("CK_ProductionPackageArtifacts_Schema", "\"SchemaVersion\" = 1");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        b.Property(x => x.SelectionHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        b.Property(x => x.Bytes).HasColumnType("bytea").IsRequired();
        b.HasOne<Quote>().WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuoteVersion>().WithMany().HasForeignKey(x => new { x.QuoteId, x.QuoteVersionId })
            .HasPrincipalKey(x => new { x.QuoteId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PublicId).IsUnique();
        b.HasIndex(x => new { x.QuoteId, x.QuoteVersionId, x.SelectionHash }).IsUnique();
    }
}
