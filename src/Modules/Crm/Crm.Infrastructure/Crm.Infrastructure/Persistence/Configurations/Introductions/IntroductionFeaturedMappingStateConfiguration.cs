using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Introductions;

public sealed class IntroductionFeaturedMappingStateConfiguration : IEntityTypeConfiguration<IntroductionFeaturedMappingState>
{
    public void Configure(EntityTypeBuilder<IntroductionFeaturedMappingState> builder)
    {
        builder.ToTable("IntroductionFeaturedMappingState", table =>
        {
            table.HasCheckConstraint("CK_IntroductionFeaturedMappingState_Id", "\"Id\" = 1");
            table.HasCheckConstraint("CK_IntroductionFeaturedMappingState_Revision", "\"Revision\" > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Revision).IsConcurrencyToken();
        builder.HasOne<IntroductionFeaturedMappingVersion>().WithMany()
            .HasForeignKey(x => x.ActiveVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(x => x.DomainEvents);
    }
}
