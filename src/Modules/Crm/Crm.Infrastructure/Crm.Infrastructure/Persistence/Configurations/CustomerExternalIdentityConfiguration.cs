using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations;

public sealed class CustomerExternalIdentityConfiguration : IEntityTypeConfiguration<CustomerExternalIdentity>
{
    public void Configure(EntityTypeBuilder<CustomerExternalIdentity> b)
    {

        b.ToTable("CustomerExternalIdentities");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomerExternalIdentityId(x)).ValueGeneratedOnAdd();
        b.Property(x => x.CustomerId).HasConversion(x => x.Value, x => new CustomerId(x));
        b.Property(x => x.Source).HasMaxLength(80);
        b.Property(x => x.Type).HasMaxLength(80);
        b.Property(x => x.ExternalId).HasMaxLength(200);
        b.HasIndex(x => new { x.Source, x.Type, x.ExternalId }).IsUnique();
        b.Ignore(x => x.DomainEvents);
    }
}
