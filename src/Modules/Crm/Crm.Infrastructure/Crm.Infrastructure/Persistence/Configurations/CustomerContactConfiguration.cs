using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations;

public sealed class CustomerContactConfiguration : IEntityTypeConfiguration<CustomerContact>
{
    public void Configure(EntityTypeBuilder<CustomerContact> b)
    {

        b.ToTable("CustomerContacts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomerContactId(x)).ValueGeneratedOnAdd();
        b.Property(x => x.CustomerId).HasConversion(x => x.Value, x => new CustomerId(x));
        b.Property(x => x.Name).HasMaxLength(300);
        b.Property(x => x.Position).HasMaxLength(200);
        b.HasIndex(x => x.PublicId).IsUnique();
        b.HasIndex(x => x.CustomerId).IsUnique().HasFilter("\"IsPrimary\" AND \"Active\"").HasDatabaseName("UX_CustomerContacts_ActivePrimary");
        b.HasMany(x => x.Channels).WithOne().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.Cascade);
        b.Ignore(x => x.DomainEvents);
    }
}
