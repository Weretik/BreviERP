using Crm.Infrastructure.Persistence.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Documents;

public sealed class DocumentSettingsMutationConfiguration : IEntityTypeConfiguration<DocumentSettingsMutationRecord>
{
    public void Configure(EntityTypeBuilder<DocumentSettingsMutationRecord> b)
    {
        b.ToTable("DocumentSettingsMutations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.Property(x => x.Operation).HasMaxLength(80).IsRequired();
        b.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.ResponseJson).HasColumnType("jsonb").IsRequired();
        b.HasIndex(x => new { x.ActorId, x.Operation, x.RequestId }).IsUnique();
    }
}
