using Crm.Domain.CustomSewing.Drafts;
using Crm.Infrastructure.Persistence.CustomSewing.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.CustomSewing;

public sealed class CustomMutationConfiguration : IEntityTypeConfiguration<CustomMutationRecord>
{
    public void Configure(EntityTypeBuilder<CustomMutationRecord> builder)
    {
        builder.ToTable("CustomSewingMutationRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Operation).HasMaxLength(80).IsRequired();
        builder.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ResponseJson).HasColumnType("jsonb").IsRequired();
        builder.HasOne<CustomSewingDraft>().WithMany().HasForeignKey(x => x.DraftPublicId)
            .HasPrincipalKey(x => x.PublicId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ActorId, x.Operation, x.RequestId }).IsUnique();
    }
}
