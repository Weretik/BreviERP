using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Reminders.Domain.Identifiers;

namespace Reminders.Domain.Chats;

/// <summary>A Telegram group or private chat the bot found. It receives reminders only after Admin approval.</summary>
public sealed class ReminderChat : BaseEntity<ReminderChatId>, IAggregateRoot
{
    public const int NameMaxLength = 200;

    private ReminderChat() { }

    public long TelegramChatId { get; private set; }
    public ReminderChatKind Kind { get; private set; }
    public string TelegramTitle { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public bool IsApproved { get; private set; }
    public bool IsBotMember { get; private set; }
    public DateTimeOffset DiscoveredAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool CanReceive => IsApproved && IsBotMember;
    public string Name => DisplayName ?? TelegramTitle;

    public static ReminderChat Discover(long telegramChatId, ReminderChatKind kind, string title, DateTimeOffset now) => new()
    {
        TelegramChatId = telegramChatId,
        Kind = kind,
        TelegramTitle = Truncate(title),
        IsBotMember = true,
        DiscoveredAt = now,
        UpdatedAt = now
    };

    public void Seen(string title, bool isBotMember, DateTimeOffset now)
    {
        TelegramTitle = Truncate(title);
        IsBotMember = isBotMember;
        UpdatedAt = now;
    }

    public void Configure(string? displayName, bool isApproved, DateTimeOffset now)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : Truncate(displayName.Trim());
        IsApproved = isApproved;
        UpdatedAt = now;
    }

    private static string Truncate(string value)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? "Без назви" : value.Trim();
        return trimmed.Length <= NameMaxLength ? trimmed : trimmed[..NameMaxLength];
    }
}
