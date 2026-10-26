using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations;

public sealed class CustomerChannelConfiguration : IEntityTypeConfiguration<CustomerChannel>
{
    public void Configure(EntityTypeBuilder<CustomerChannel> b)
    {

        b.ToTable("ContactChannels", t => t.HasCheckConstraint("CK_ContactChannels_Kind", "\"Kind\" IN ('email','phone')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CustomerChannelId(x)).ValueGeneratedOnAdd();
        b.Property(x => x.ContactId).HasConversion(x => x.Value, x => new CustomerContactId(x));
        b.Property(x => x.Kind).HasMaxLength(10);
        b.Property(x => x.Value).HasMaxLength(254);
        b.Property(x => x.NormalizedValue).HasMaxLength(254);
        b.HasIndex(x => new { x.ContactId, x.Kind, x.NormalizedValue }).IsUnique();
        b.HasIndex(x => x.NormalizedValue);
        b.Ignore(x => x.DomainEvents);
    }
}
