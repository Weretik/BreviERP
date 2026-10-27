using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuotePriceBasisRecordConfiguration : IEntityTypeConfiguration<QuotePriceBasisRecord>
{
    public void Configure(EntityTypeBuilder<QuotePriceBasisRecord> builder)
    {
        builder.ToTable("CalculationBases", table =>
            table.HasCheckConstraint("CK_CalculationBases_Expiry", "\"ExpiresAt\" > \"CreatedAt\""));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SnapshotJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.ActorId, x.RequestId }).IsUnique();
        builder.HasIndex(x => x.ExpiresAt);
    }
}
