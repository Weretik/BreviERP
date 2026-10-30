using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Issues;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Documents;

public sealed class QuotePdfArtifactConfiguration : IEntityTypeConfiguration<QuotePdfArtifact>
{
    public void Configure(EntityTypeBuilder<QuotePdfArtifact> b)
    {
        b.ToTable("QuotePdfArtifacts", "crm", t =>
        {
            t.HasCheckConstraint("CK_QuotePdfArtifacts_Status",
                "\"Status\" IN ('queued','processing','succeeded','failed')");
            t.HasCheckConstraint("CK_QuotePdfArtifacts_Bytes",
                "(\"PdfBytes\" IS NULL OR octet_length(\"PdfBytes\") <= 20971520)");
            t.HasCheckConstraint("CK_QuotePdfArtifacts_ContentState",
                "(\"Status\" = 'succeeded' AND \"PdfBytes\" IS NOT NULL AND \"Sha256\" IS NOT NULL AND \"Length\" > 0 AND \"GeneratedAt\" IS NOT NULL) OR (\"Status\" <> 'succeeded' AND \"PdfBytes\" IS NULL)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        b.Property(x => x.IssuerVersionId).HasConversion(x => x.Value, x => new IssuerSettingsVersionId(x));
        b.Property(x => x.TemplateVersionId).HasConversion(x => x.Value, x => new DocumentTemplateVersionId(x));
        b.Property(x => x.RenderKey).HasMaxLength(128).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired().IsConcurrencyToken();
        b.Property(x => x.Sha256).HasMaxLength(64);
        b.Property(x => x.ErrorCode).HasMaxLength(80);
        b.Property(x => x.PdfBytes).HasColumnType("bytea");
        b.Property(x => x.LeaseToken).IsConcurrencyToken();
        b.HasOne<Quote>().WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuoteVersion>().WithMany()
            .HasForeignKey(x => new { x.QuoteId, x.QuoteVersionId })
            .HasPrincipalKey(x => new { x.QuoteId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuoteIssue>().WithMany()
            .HasForeignKey(x => new { x.QuoteId, x.QuoteVersionId,
                x.IssuerVersionId, x.TemplateVersionId })
            .HasPrincipalKey(x => new { x.QuoteId, x.QuoteVersionId,
                x.IssuerVersionId, x.TemplateVersionId })
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<IssuerSettingsVersion>().WithMany().HasForeignKey(x => x.IssuerVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DocumentTemplateVersion>().WithMany().HasForeignKey(x => x.TemplateVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PublicId).IsUnique();
        b.HasIndex(x => new { x.QuoteId, x.QuoteVersionId, x.IssuerVersionId,
            x.TemplateVersionId, x.RenderKey }).IsUnique();
        b.HasIndex(x => new { x.Status, x.LeaseUntil, x.CreatedAt });
    }
}
