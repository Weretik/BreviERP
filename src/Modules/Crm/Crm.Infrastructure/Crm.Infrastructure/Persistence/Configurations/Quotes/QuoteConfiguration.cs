using Crm.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> b)
    {
        b.ToTable("Quotes", t =>
        {
            t.HasCheckConstraint("CK_Quotes_Version", "\"Version\" > 0");
            t.HasCheckConstraint("CK_Quotes_NumberMonth", "\"NumberMonth\" BETWEEN 1 AND 12");
            t.HasCheckConstraint("CK_Quotes_NumberSequence", "\"NumberSequence\" > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Number).HasMaxLength(QuoteLimits.NumberMaxLength);
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.InternalName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.RecipientName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.CompanyName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.ClientComment).HasMaxLength(QuoteLimits.TextMaxLength);
        b.Property(x => x.ManagerNote).HasMaxLength(QuoteLimits.TextMaxLength);
        b.Property(x => x.AuthorName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Property(x => x.ResponsibleName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Ignore(x => x.Introduction);
        b.Property(x => x.IntroLayout).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RecipientEmail).HasMaxLength(QuoteLimits.EmailMaxLength);
        b.Property(x => x.LetterName).HasMaxLength(QuoteLimits.LetterNameMaxLength);
        b.Property(x => x.ArchivedByName).HasMaxLength(QuoteLimits.NameMaxLength);
        b.Ignore(x => x.IsArchived);
        b.Property(x => x.Version).IsConcurrencyToken();

        b.HasIndex(x => x.Number).IsUnique();
        b.HasIndex(x => new { x.NumberYear, x.NumberMonth, x.NumberSequence }).IsUnique();
        b.HasIndex(x => new { x.ResponsibleUserId, x.UpdatedAt });
        b.HasIndex(x => x.UpdatedAt);
        b.HasIndex(x => x.ArchivedAt);
        b.HasIndex(x => x.CustomerId);

        // A Crm customer; the KeepinCRM sync keeps customers that a КП uses (CustomerKeepinSyncStore.ListInUseAsync).
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field);

        // History lines are only added through the КП and read by GetQuoteHistory; the КП never loads them.
        b.HasMany(x => x.Events).WithOne().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Events).HasField("_events").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
