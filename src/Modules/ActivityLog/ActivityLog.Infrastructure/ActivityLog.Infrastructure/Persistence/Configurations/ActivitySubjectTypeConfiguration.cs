using ActivityLog.Domain.Entries;
using ActivityLog.Domain.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActivityLog.Infrastructure.Persistence.Configurations;

/// <summary>Table <c>activity_log.ActivitySubjectTypes</c>: one row per object type seen in the journal.</summary>
public sealed class ActivitySubjectTypeConfiguration : IEntityTypeConfiguration<ActivitySubjectType>
{
    public void Configure(EntityTypeBuilder<ActivitySubjectType> builder)
    {
        builder.ToTable("ActivitySubjectTypes");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasMaxLength(ActivityEntry.SubjectTypeMaxLength);
        builder.Property(x => x.Title).HasMaxLength(ActivityEntry.SubjectTypeTitleMaxLength).IsRequired();
    }
}
