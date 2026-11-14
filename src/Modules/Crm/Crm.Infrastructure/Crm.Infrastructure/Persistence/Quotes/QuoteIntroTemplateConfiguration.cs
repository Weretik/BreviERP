using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Quotes;

/// <summary>«Готові шаблони» of introductions; the pages are one jsonb document.</summary>
public sealed class QuoteIntroTemplateConfiguration : IEntityTypeConfiguration<QuoteIntroTemplate>
{
    public void Configure(EntityTypeBuilder<QuoteIntroTemplate> b)
    {
        b.ToTable("QuoteIntroTemplates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Name).HasMaxLength(QuoteIntroTemplateLimits.NameMaxLength);
        b.Property(x => x.NameKey).HasMaxLength(QuoteIntroTemplateLimits.NameMaxLength);
        b.Property(x => x.Layout).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.LetterName).HasMaxLength(QuoteLimits.LetterNameMaxLength);
        b.Property(x => x.AuthorName).HasMaxLength(QuoteLimits.NameMaxLength);
        JsonColumns.Map(b.Property(x => x.Pages));
        b.HasIndex(x => x.NameKey).IsUnique();
    }
}
