using ActivityLog.Domain.Entries;
using ActivityLog.Domain.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActivityLog.Infrastructure.Persistence.Configurations;

/// <summary>Table <c>activity_log.ActivityActors</c>: one row per person seen in the journal.</summary>
public sealed class ActivityActorConfiguration : IEntityTypeConfiguration<ActivityActor>
{
    public void Configure(EntityTypeBuilder<ActivityActor> builder)
    {
        builder.ToTable("ActivityActors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(ActivityEntry.ActorNameMaxLength).IsRequired();
    }
}
