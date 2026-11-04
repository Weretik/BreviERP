using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Sending;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

/// <summary>One row per staff member (Identity user id; no cross-module foreign key).</summary>
public sealed class StaffMailboxConfiguration : IEntityTypeConfiguration<StaffMailbox>
{
    public void Configure(EntityTypeBuilder<StaffMailbox> b)
    {
        b.ToTable("StaffMailboxes");
        b.HasKey(x => x.UserId);
        b.Property(x => x.UserId).ValueGeneratedNever();
        b.Property(x => x.Email).HasMaxLength(QuoteSendingLimits.EmailMaxLength);
        b.Property(x => x.ProtectedRefreshToken).HasMaxLength(QuoteSendingLimits.TokenMaxLength);
        b.Property(x => x.ConnectedByName).HasMaxLength(QuoteLimits.NameMaxLength);
    }
}
