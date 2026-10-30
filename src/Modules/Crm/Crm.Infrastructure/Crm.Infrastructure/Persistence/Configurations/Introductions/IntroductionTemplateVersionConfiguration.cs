using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Introductions;

public sealed class IntroductionTemplateVersionConfiguration : IEntityTypeConfiguration<IntroductionTemplateVersion>
{
    public void Configure(EntityTypeBuilder<IntroductionTemplateVersion> builder)
    {
        builder.ToTable("IntroductionTemplateVersions", table =>
            table.HasCheckConstraint("CK_IntroductionTemplateVersion_Number", "\"Number\" > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new IntroductionTemplateVersionId(x))
            .ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.TemplateId).HasConversion(x => x.Value, x => new IntroductionTemplateId(x));
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Payload).HasConversion(
            x => IntroductionTemplatePayloadJson.Serialize(x),
            x => IntroductionTemplatePayloadJson.Deserialize(x)).HasColumnType("jsonb").IsRequired();
        builder.HasAlternateKey(x => x.PublicId);
        builder.HasIndex(x => new { x.TemplateId, x.Number }).IsUnique();
        builder.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(x => x.DomainEvents);
    }
}
