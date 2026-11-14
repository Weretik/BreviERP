using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Purchasing.Domain.Sources;

namespace Purchasing.Infrastructure.Persistence.Sources;

public sealed class PurchaseSourceRevisionConfiguration : IEntityTypeConfiguration<PurchaseSourceRevision>
{
    public void Configure(EntityTypeBuilder<PurchaseSourceRevision> b)
    {
        b.ToTable("SourceRevisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(64);
        b.Property(x => x.SourceId).HasMaxLength(64);
        b.HasIndex(x => x.SourceId);
        b.HasOne<PurchaseSourceRow>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
    }
}
