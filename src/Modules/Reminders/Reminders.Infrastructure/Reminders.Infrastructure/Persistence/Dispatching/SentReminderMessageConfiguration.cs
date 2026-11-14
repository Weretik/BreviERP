using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reminders.Domain.Chats;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Occurrences;

namespace Reminders.Infrastructure.Persistence.Dispatching;

public sealed class SentReminderMessageConfiguration : IEntityTypeConfiguration<SentReminderMessage>
{
    public void Configure(EntityTypeBuilder<SentReminderMessage> b)
    {
        b.ToTable("SentMessages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.OccurrenceId).HasConversion(x => x.Value, x => new ReminderOccurrenceId(x));
        b.Property(x => x.ChatId).HasConversion(x => x.Value, x => new ReminderChatId(x));
        // At most one card per period per day, even if the job is started twice.
        b.HasIndex(x => new { x.OccurrenceId, x.SentOn }).IsUnique();
        b.HasIndex(x => new { x.OccurrenceId, x.HasButtons });
        b.HasOne<ReminderOccurrence>().WithMany().HasForeignKey(x => x.OccurrenceId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<ReminderChat>().WithMany().HasForeignKey(x => x.ChatId).OnDelete(DeleteBehavior.Restrict);
    }
}
