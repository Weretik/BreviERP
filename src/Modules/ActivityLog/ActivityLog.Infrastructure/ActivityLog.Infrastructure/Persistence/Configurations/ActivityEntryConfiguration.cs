using System.Text.Json;
using ActivityLog.Domain.Entries;
using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActivityLog.Infrastructure.Persistence.Configurations;

/// <summary>Table <c>activity_log.ActivityEntries</c> (ACT-001 «Data»).</summary>
public sealed class ActivityEntryConfiguration : IEntityTypeConfiguration<ActivityEntry>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<ActivityEntry> builder)
    {
        builder.ToTable("ActivityEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ActorName).HasMaxLength(ActivityEntry.ActorNameMaxLength).IsRequired();
        builder.Property(x => x.ActorRoles).HasMaxLength(ActivityEntry.RolesMaxLength).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(ActivityEntry.ActionMaxLength).IsRequired();
        builder.Property(x => x.SubjectType).HasMaxLength(ActivityEntry.SubjectTypeMaxLength).IsRequired();
        builder.Property(x => x.SubjectTypeTitle).HasMaxLength(ActivityEntry.SubjectTypeTitleMaxLength).IsRequired();
        builder.Property(x => x.SubjectId).HasMaxLength(ActivityEntry.SubjectIdMaxLength).IsRequired();
        builder.Property(x => x.SubjectTitle).HasMaxLength(ActivityEntry.SubjectTitleMaxLength).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(ActivityEntry.SummaryMaxLength);
        builder.Property(x => x.SourceModule).HasMaxLength(ActivityEntry.SourceModuleMaxLength).IsRequired();
        builder.Property(x => x.Changes)
            .HasColumnType("jsonb")
            .HasConversion(
                changes => JsonSerializer.Serialize(changes, Json),
                json => JsonSerializer.Deserialize<ActivityChange[]>(json, Json) ?? Array.Empty<ActivityChange>(),
                new ValueComparer<IReadOnlyList<ActivityChange>>(
                    (a, b) => a!.SequenceEqual(b!),
                    changes => changes.Aggregate(0, (hash, change) => HashCode.Combine(hash, change)),
                    changes => changes.ToArray()))
            .IsRequired();
        builder.Ignore(x => x.RoleNames);

        // Database rules 3: equality columns first, then the sort, then Id.
        builder.HasIndex(x => new { x.OccurredAt, x.Id }).IsDescending(true, true);
        builder.HasIndex(x => new { x.ActorId, x.OccurredAt }).IsDescending(false, true);
        builder.HasIndex(x => new { x.SubjectType, x.OccurredAt }).IsDescending(false, true);
        builder.HasSearchText(nameof(ActivityEntry.SubjectTitle), nameof(ActivityEntry.Summary));
    }
}
