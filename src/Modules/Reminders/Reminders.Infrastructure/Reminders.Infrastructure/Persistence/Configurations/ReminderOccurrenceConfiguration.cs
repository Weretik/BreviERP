using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Tasks;

namespace Reminders.Infrastructure.Persistence.Configurations;

public sealed class ReminderOccurrenceConfiguration : IEntityTypeConfiguration<ReminderOccurrence>
{
    public void Configure(EntityTypeBuilder<ReminderOccurrence> b)
    {
        b.ToTable("Occurrences");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ReminderOccurrenceId(x)).ValueGeneratedOnAdd();
        b.Property(x => x.TaskId).HasConversion(x => x.Value, x => new ReminderTaskId(x));
        b.Property(x => x.Periodicity).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.OwnsOne(x => x.ClosedBy, a => ConfigureActor(a, "ClosedBy"));
        b.OwnsOne(x => x.PostponedBy, a => ConfigureActor(a, "PostponedBy"));
        b.HasMany(x => x.Actions).WithOne().HasForeignKey("OccurrenceId").IsRequired().OnDelete(DeleteBehavior.Cascade);
        b.HasOne<ReminderTask>().WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Restrict);
        b.Navigation(x => x.Actions).UsePropertyAccessMode(PropertyAccessMode.Field);
        // One occurrence per task and period; the daily job relies on it to stay idempotent.
        b.HasIndex(x => new { x.TaskId, x.PeriodStart }).IsUnique();
        b.HasIndex(x => new { x.Status, x.DueDate });
        b.Ignore(x => x.Period);
        b.Ignore(x => x.DomainEvents);
    }

    internal static void ConfigureActor<T>(OwnedNavigationBuilder<T, ReminderActor> actor, string prefix) where T : class
    {
        actor.Property(x => x.Name).HasColumnName(prefix + "Name").HasMaxLength(ReminderActor.NameMaxLength);
        actor.Property(x => x.TelegramUserId).HasColumnName(prefix + "TelegramUserId");
        actor.Property(x => x.AdminUserId).HasColumnName(prefix + "AdminUserId");
    }
}
