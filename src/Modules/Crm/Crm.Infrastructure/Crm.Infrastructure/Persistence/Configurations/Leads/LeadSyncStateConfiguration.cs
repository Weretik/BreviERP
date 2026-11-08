using Crm.Domain.Leads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Leads;

public sealed class LeadSyncStateConfiguration : IEntityTypeConfiguration<LeadSyncState>
{
    public void Configure(EntityTypeBuilder<LeadSyncState> b)
    {
        b.ToTable("LeadSyncStates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(40);
        b.Property(x => x.Error).HasMaxLength(LeadLimits.LongText);
    }
}
