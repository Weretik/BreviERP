using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations;

public sealed class CustomerMutationRecordConfiguration : IEntityTypeConfiguration<CustomerMutationRecord>
{
    public void Configure(EntityTypeBuilder<CustomerMutationRecord> b)
    {

        b.ToTable("MutationRequests");
        b.HasKey(x => x.Id);
        b.Property(x => x.Operation).HasMaxLength(80);
        b.Property(x => x.PayloadHash).HasMaxLength(64);
        b.Property(x => x.IntentJson).HasColumnType("jsonb");
        b.Property(x => x.ResponseJson).HasColumnType("jsonb");
        b.HasIndex(x => new { x.ActorId, x.Operation, x.RequestId }).IsUnique();
        b.HasIndex(x => x.CustomerPublicId);
    }
}
