using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Infrastructure.Persistence.CustomSewing.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.CustomSewing;

public sealed class CustomCalculationConfiguration : IEntityTypeConfiguration<CustomCalculationRecord>
{
    public void Configure(EntityTypeBuilder<CustomCalculationRecord> builder)
    {
        builder.ToTable("CustomSewingCalculations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.DraftId).HasConversion(x => x.Value, x => new CustomDraftId(x));
        builder.Property(x => x.BasisJson).HasColumnType("jsonb").IsRequired();
        builder.HasOne<CustomSewingDraft>().WithMany().HasForeignKey(x => x.DraftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => new { x.DraftId, x.Revision, x.ActorId });
    }
}
