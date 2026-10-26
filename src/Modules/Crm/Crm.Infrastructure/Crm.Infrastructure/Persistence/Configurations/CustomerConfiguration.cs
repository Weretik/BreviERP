using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {

        b.ToTable("Customers", t => { t.HasCheckConstraint("CK_Customers_Revision", "\"Revision\" > 0"); t.HasCheckConstraint("CK_Customers_Kind", "\"Kind\" IN ('organization','person')"); });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomerId(x)).ValueGeneratedOnAdd();
        b.Property(x => x.OwnerId).HasConversion(x => x.Value, x => new CrmUserId(x));
        b.Property(x => x.MergedIntoId).HasConversion(x => x.HasValue ? x.Value.Value : (long?)null, x => x.HasValue ? new CustomerId(x.Value) : null);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.MergedIntoId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Kind).HasMaxLength(20);
        b.Property(x => x.DisplayName).HasMaxLength(300);
        b.Property(x => x.NormalizedName).HasMaxLength(300);
        b.Property(x => x.LegalName).HasMaxLength(500);
        b.Property(x => x.TaxIdentity).HasMaxLength(80);
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasIndex(x => x.PublicId).IsUnique();
        b.HasIndex(x => new { x.OwnerId, x.Active, x.UpdatedAt });
        b.HasIndex(x => x.NormalizedName);
        b.HasIndex(x => x.TaxIdentity);
        b.HasMany(x => x.Contacts).WithOne().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.ExternalIdentities).WithOne().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.DomainEvents);
    }
}
