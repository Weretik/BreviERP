using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reminders.Domain.Chats;
using Reminders.Domain.Identifiers;

namespace Reminders.Infrastructure.Persistence.Configurations;

public sealed class ReminderChatConfiguration : IEntityTypeConfiguration<ReminderChat>
{
    public void Configure(EntityTypeBuilder<ReminderChat> b)
    {
        b.ToTable("Chats");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ReminderChatId(x)).ValueGeneratedOnAdd();
        b.Property(x => x.Kind).HasConversion<int>();
        b.Property(x => x.TelegramTitle).HasMaxLength(ReminderChat.NameMaxLength).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(ReminderChat.NameMaxLength);
        b.HasIndex(x => x.TelegramChatId).IsUnique();
        b.Ignore(x => x.CanReceive);
        b.Ignore(x => x.GetsSiteRequests);
        b.Ignore(x => x.Name);
    }
}
