using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Sending;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Quotes;

public sealed class QuoteSendingConfiguration : IEntityTypeConfiguration<QuoteSending>
{
    public void Configure(EntityTypeBuilder<QuoteSending> b)
    {
        b.ToTable("QuoteSendings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.SenderName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.FromEmail).HasMaxLength(QuoteSendingLimits.EmailMaxLength);
        b.Property(x => x.To).HasMaxLength(QuoteSendingLimits.RecipientsMaxLength);
        b.Property(x => x.Subject).HasMaxLength(QuoteSendingLimits.SubjectMaxLength);
        b.Property(x => x.ProviderMessageId).HasMaxLength(200);
        b.HasOne<Quote>().WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.QuoteId, x.SentAt });
    }
}
