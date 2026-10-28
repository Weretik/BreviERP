using Crm.Domain.CustomSewing.Templates;
using Crm.Infrastructure.Persistence.Templates.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations.Templates;

public sealed class TemplateMutationConfiguration : IEntityTypeConfiguration<TemplateMutationRecord>
{
    public void Configure(EntityTypeBuilder<TemplateMutationRecord> b)
    {
        b.ToTable("CustomSewingTemplateMutationRequests");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.Operation).HasMaxLength(80).IsRequired();
        b.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.ResponseJson).HasColumnType("jsonb").IsRequired();
        b.HasIndex(x => new { x.ActorId, x.Operation, x.RequestId }).IsUnique();
        b.HasOne<CustomTemplate>().WithMany().HasForeignKey(x => x.TemplatePublicId).HasPrincipalKey(x => x.PublicId).OnDelete(DeleteBehavior.Restrict);
    }
}
