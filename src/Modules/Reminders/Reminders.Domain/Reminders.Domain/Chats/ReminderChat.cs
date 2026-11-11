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

    /// <summary>The group also gets the requests of the public site (LEAD-002); only while it can receive.</summary>
    public bool ReceivesSiteRequests { get; private set; }
    public DateTimeOffset DiscoveredAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool CanReceive => IsApproved && IsBotMember;
    public bool GetsSiteRequests => ReceivesSiteRequests && CanReceive;
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

    /// <param name="receivesSiteRequests">Null keeps the current choice.</param>
    public void Configure(string? displayName, bool isApproved, DateTimeOffset now, bool? receivesSiteRequests = null)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : Truncate(displayName.Trim());
        IsApproved = isApproved;
        ReceivesSiteRequests = receivesSiteRequests ?? ReceivesSiteRequests;
        UpdatedAt = now;
    }

    private static string Truncate(string value)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? "Без назви" : value.Trim();
        return trimmed.Length <= NameMaxLength ? trimmed : trimmed[..NameMaxLength];
    }
}
