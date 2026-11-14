using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reminders.Domain.Chats;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Tasks;


namespace Reminders.Infrastructure.Persistence.Configurations;

public sealed class ReminderTaskConfiguration : IEntityTypeConfiguration<ReminderTask>
{
    public void Configure(EntityTypeBuilder<ReminderTask> b)
    {
        b.ToTable("Tasks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ReminderTaskId(x)).ValueGeneratedOnAdd();
        b.Property(x => x.Title).HasMaxLength(ReminderTask.TitleMaxLength).IsRequired();
        b.Property(x => x.Organization).HasConversion<int>();
        b.Property(x => x.ChatId).HasConversion(x => x!.Value.Value, x => new ReminderChatId(x));
        b.ComplexProperty(x => x.Schedule, s =>
        {
            s.Property(x => x.Periodicity).HasColumnName("Periodicity").HasConversion<int>();
            s.Property(x => x.LeadDays).HasColumnName("LeadDays");
            s.ComplexProperty(x => x.Due, d =>
            {
                d.Property(x => x.Kind).HasColumnName("DueRule").HasConversion<int>();
                d.Property(x => x.Day).HasColumnName("DueDay");
                d.Property(x => x.MonthOffset).HasColumnName("DueMonthOffset");
            });
        });
        b.HasOne<ReminderChat>().WithMany().HasForeignKey(x => x.ChatId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.CanSend);
    }
}
