using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Issues;
using Crm.Domain.Quotes.Versions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteIssueConfiguration : IEntityTypeConfiguration<QuoteIssue>
{
    public void Configure(EntityTypeBuilder<QuoteIssue> b)
    {
        b.ToTable("QuoteIssues", t =>
        {
            t.HasCheckConstraint("CK_QuoteIssues_QuoteVersionId", "\"QuoteVersionId\" > 0");
            t.HasCheckConstraint("CK_QuoteIssues_Number", "length(\"Number\") > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        b.Property(x => x.IssuerVersionId).HasConversion(x => x.Value, x => new IssuerSettingsVersionId(x));
        b.Property(x => x.TemplateVersionId).HasConversion(x => x.Value, x => new DocumentTemplateVersionId(x));
        b.Property(x => x.Number).HasMaxLength(80).IsRequired();
        b.HasOne<Quote>().WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuoteVersion>().WithMany()
            .HasForeignKey(x => new { x.QuoteId, x.QuoteVersionId })
            .HasPrincipalKey(x => new { x.QuoteId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<IssuerSettingsVersion>().WithMany()
            .HasForeignKey(x => x.IssuerVersionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DocumentTemplateVersion>().WithMany()
            .HasForeignKey(x => x.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.QuoteId, x.QuoteVersionId }).IsUnique();
    }
}
