using Crm.Domain.Delivery;
using Crm.Domain.Documents.EmailTemplates;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence.Delivery;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Delivery;

public sealed class QuoteDeliveryConfiguration : IEntityTypeConfiguration<QuoteDelivery>
{
    public void Configure(EntityTypeBuilder<QuoteDelivery> b)
    {
        b.ToTable("QuoteDeliveries", "crm", t => t.HasCheckConstraint("CK_QuoteDeliveries_Bytes", "octet_length(\"PdfBytes\") BETWEEN 5 AND 20971520"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new QuoteDeliveryId(x)).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        b.Property(x => x.EmailTemplateVersionId).HasConversion(x => x.Value, x => new EmailTemplateVersionId(x));
        b.Property(x => x.ProviderKey).HasMaxLength(80).IsRequired();
        b.Property(x => x.SenderAddress).HasMaxLength(254).IsRequired();
        b.Property(x => x.RecipientAddress).HasMaxLength(254).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        b.Property(x => x.PlainText).HasMaxLength(3000).IsRequired();
        b.Property(x => x.PdfSha256).HasMaxLength(64).IsRequired();
        b.Property(x => x.PdfBytes).HasColumnType("bytea").IsRequired();
        b.HasIndex(x => x.PublicId).IsUnique();
        b.HasIndex(x => new { x.QuoteId, x.CreatedAt });
        b.HasAlternateKey(x => new { x.QuoteId, x.Id });
        b.HasAlternateKey(x => new { x.QuoteId, x.QuoteVersionId, x.Id });
        b.HasOne<Quote>().WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuoteVersion>().WithMany().HasForeignKey(x => new { x.QuoteId, x.QuoteVersionId })
            .HasPrincipalKey(x => new { x.QuoteId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuotePdfArtifact>().WithMany().HasForeignKey(x => new { x.QuoteId, x.QuoteVersionId, x.ArtifactId })
            .HasPrincipalKey(x => new { x.QuoteId, x.QuoteVersionId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<EmailTemplateVersion>().WithMany().HasForeignKey(x => x.EmailTemplateVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
