using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Infrastructure.Persistence.Documents.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Production;

public sealed class ProductionPackageMutationConfiguration : IEntityTypeConfiguration<ProductionPackageMutationRecord>
{
    public void Configure(EntityTypeBuilder<ProductionPackageMutationRecord> b)
    {
        b.ToTable("ProductionPackageRequests", "crm", t => t.HasCheckConstraint("CK_ProductionPackageRequests_Actor", "\"ActorId\" > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        b.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        b.HasOne<Quote>().WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ProductionPackageArtifact>().WithMany().HasForeignKey(x => new { x.QuoteId, x.PackageArtifactId }).HasPrincipalKey(x => new { x.QuoteId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ActorId, x.RequestId }).IsUnique();
    }
}
