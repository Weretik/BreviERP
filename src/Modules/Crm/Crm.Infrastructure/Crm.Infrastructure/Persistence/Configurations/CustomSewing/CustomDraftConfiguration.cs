using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Versions;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.CustomSewing;

public sealed class CustomDraftConfiguration : IEntityTypeConfiguration<CustomSewingDraft>
{
    public void Configure(EntityTypeBuilder<CustomSewingDraft> builder)
    {
        builder.ToTable("CustomSewingDrafts", table => table.HasCheckConstraint("CK_CustomDraft_Revision", "\"Revision\" > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomDraftId(x)).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.OwnerId).HasConversion(x => x.Value, x => new CrmUserId(x));
        builder.Property(x => x.CustomerId).HasConversion(x => x.HasValue ? x.Value.Value : (long?)null,
            x => x.HasValue ? new CustomerId(x.Value) : null);
        builder.Property(x => x.QuoteId).HasConversion(x => x.HasValue ? x.Value.Value : (long?)null,
            x => x.HasValue ? new QuoteId(x.Value) : null);
        builder.Property(x => x.CurrentVersionId).HasConversion(x => x.HasValue ? x.Value.Value : (long?)null,
            x => x.HasValue ? new CustomVersionId(x.Value) : null);
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ProductType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Season).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Revision).IsConcurrencyToken();
        CustomJsonProperty.Configure(builder.Property(x => x.Configuration));
        CustomJsonProperty.ConfigureOptional(builder.Property(x => x.TemplateOrigin));
        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Quote).WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CurrentVersion).WithMany().HasForeignKey(x => new { x.Id, x.CurrentVersionId })
            .HasPrincipalKey(x => new { x.DraftId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        builder.HasAlternateKey(x => x.PublicId);
        builder.HasIndex(x => new { x.OwnerId, x.UpdatedAt, x.Id });
        builder.Ignore(x => x.DomainEvents);
    }
}
