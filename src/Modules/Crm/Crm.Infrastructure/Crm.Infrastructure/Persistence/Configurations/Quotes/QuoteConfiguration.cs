using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Versions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes", table =>
        {
            table.HasCheckConstraint("CK_Quotes_Revision", "\"Revision\" > 0");
            table.HasCheckConstraint("CK_Quotes_State", "\"State\" IN ('draft','ready','sent')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new QuoteId(x))
            .ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.CustomerId).HasConversion(
            x => x.HasValue ? x.Value.Value : (long?)null,
            x => x.HasValue ? new CustomerId(x.Value) : null);
        builder.Property(x => x.OwnerId).HasConversion(x => x.Value, x => new CrmUserId(x));
        builder.Property(x => x.CreatedBy).HasConversion(x => x.Value, x => new CrmUserId(x));
        builder.Property(x => x.RecipientName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.State).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Number).HasMaxLength(80);
        builder.Property(x => x.Revision).IsConcurrencyToken();
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<QuoteVersion>().WithMany()
            .HasForeignKey(x => new { x.Id, x.CurrentVersionId })
            .HasPrincipalKey(x => new { x.QuoteId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasOne<QuoteVersion>().WithMany()
            .HasForeignKey(x => new { x.Id, x.IssuedVersionId })
            .HasPrincipalKey(x => new { x.QuoteId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.Number).IsUnique().HasFilter("\"Number\" IS NOT NULL");
        builder.HasIndex(x => new { x.OwnerId, x.Archived, x.UpdatedAt, x.Id });
        builder.HasIndex(x => new { x.SourceQuotePublicId, x.Id });
        builder.HasIndex(x => new { x.CreatedAt, x.Id });
        builder.Ignore(x => x.DomainEvents);
    }
}
