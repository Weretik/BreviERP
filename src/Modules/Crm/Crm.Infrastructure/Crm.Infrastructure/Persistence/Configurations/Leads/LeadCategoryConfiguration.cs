using Crm.Domain.Leads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Crm.Infrastructure.Persistence.Configurations.Leads;

public sealed class LeadCategoryConfiguration : IEntityTypeConfiguration<LeadCategory>
{
    public void Configure(EntityTypeBuilder<LeadCategory> b)
    {
        b.ToTable("LeadCategories");
        b.HasKey(x => x.Code);
        b.Property(x => x.Code).HasMaxLength(LeadLimits.CategoryMaxLength);
        b.Property(x => x.Description).HasMaxLength(LeadLimits.CategoryDescriptionMaxLength);
    }
}
