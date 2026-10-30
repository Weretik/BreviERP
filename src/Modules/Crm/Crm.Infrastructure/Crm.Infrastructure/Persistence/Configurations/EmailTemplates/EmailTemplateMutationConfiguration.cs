using Crm.Infrastructure.Persistence.Documents.EmailTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.EmailTemplates;
public sealed class EmailTemplateMutationConfiguration : IEntityTypeConfiguration<EmailTemplateMutationRecord>
{
    public void Configure(EntityTypeBuilder<EmailTemplateMutationRecord> b)
    {
        b.ToTable("EmailTemplateMutations"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Operation).HasMaxLength(60).IsRequired(); b.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.ResponseJson).HasColumnType("jsonb").IsRequired();
        b.HasIndex(x => new { x.ActorId, x.Operation, x.RequestId }).IsUnique();
    }
}
