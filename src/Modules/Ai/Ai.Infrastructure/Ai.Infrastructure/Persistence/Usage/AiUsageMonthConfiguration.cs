using Ai.Domain.Usage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ai.Infrastructure.Persistence.Usage;

public sealed class AiUsageMonthConfiguration : IEntityTypeConfiguration<AiUsageMonth>
{
    public void Configure(EntityTypeBuilder<AiUsageMonth> b)
    {
        b.ToTable("UsageMonths");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(7);
        b.Property(x => x.CostUsd).HasPrecision(12, 6);
    }
}
