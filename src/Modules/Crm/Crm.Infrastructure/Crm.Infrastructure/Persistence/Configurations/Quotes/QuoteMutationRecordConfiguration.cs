using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteMutationRecordConfiguration : IEntityTypeConfiguration<QuoteMutationRecord>
{
    public void Configure(EntityTypeBuilder<QuoteMutationRecord> builder)
    {
        builder.ToTable("QuoteMutationRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Operation).HasMaxLength(80).IsRequired();
        builder.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ResponseJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.ActorId, x.Operation, x.RequestId }).IsUnique();
        builder.HasIndex(x => x.QuotePublicId);
    }
}
