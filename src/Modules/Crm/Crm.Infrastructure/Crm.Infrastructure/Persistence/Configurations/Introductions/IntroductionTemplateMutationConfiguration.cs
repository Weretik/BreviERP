using Crm.Domain.Quotes.Introductions;
using Crm.Infrastructure.Persistence.Introductions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Introductions;

public sealed class IntroductionTemplateMutationConfiguration : IEntityTypeConfiguration<IntroductionTemplateMutationRecord>
{
    public void Configure(EntityTypeBuilder<IntroductionTemplateMutationRecord> builder)
    {
        builder.ToTable("IntroductionTemplateMutationRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Operation).HasMaxLength(80).IsRequired();
        builder.Property(x => x.IntentHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ResponseJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.ActorId, x.Operation, x.RequestId }).IsUnique();
        builder.HasOne<IntroductionTemplate>().WithMany().HasForeignKey(x => x.TemplatePublicId)
            .HasPrincipalKey(x => x.PublicId).OnDelete(DeleteBehavior.Restrict);
    }
}
