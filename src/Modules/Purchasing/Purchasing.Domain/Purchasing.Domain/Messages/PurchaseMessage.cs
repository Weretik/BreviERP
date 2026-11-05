namespace Purchasing.Domain.Messages;

/// <summary>
/// One Telegram message of the purchase sheet; several sheet rows (UIDs) may belong to it. Source fields are refreshed by
/// every sync; the supplier link, the note and their version are Brevi's own and the sync never touches them.
/// </summary>
public sealed class PurchaseMessage
{
    public const int ReviewNoteMaxLength = 1_000;

    private PurchaseMessage() { }

    public PurchaseMessage(string id, string telegramKey, MessageSource source, DateTimeOffset now)
    {
        Id = id;
        TelegramKey = telegramKey;
        ReviewVersion = 0;
        Apply(source, now);
    }

    /// <summary>SHA-256 of «telegram:» + <see cref="TelegramKey"/>, lower-case hex.</summary>
    public string Id { get; private set; } = null!;

    /// <summary>trim(ChatId) + ':' + trim(MessageId).</summary>
    public string TelegramKey { get; private set; } = null!;

    /// <summary>The DateTime cell exactly as written in the sheet.</summary>
    public string? MessageAt { get; private set; }

    /// <summary><see cref="MessageAt"/> recognised as a date and time, only for sorting; null when not recognised.</summary>
    public DateTime? MessageDate { get; private set; }

    public string Text { get; private set; } = null!;
    public string StatusOriginal { get; private set; } = null!;

    /// <summary>StatusDate: when the payment mark was set.</summary>
    public string? MarkAt { get; private set; }

    /// <summary>FromUserStatus: who set the payment mark, not necessarily the author of the message.</summary>
    public string? MarkBy { get; private set; }

    public PaymentState PaymentState { get; private set; }

    /// <summary>Brevi supplier linked by hand (Reference module id, no database link across modules).</summary>
    public int? SupplierId { get; private set; }

    public string? ReviewNote { get; private set; }

    /// <summary>Optimistic version of the manual supplier link and note; starts at 0.</summary>
    public int ReviewVersion { get; private set; }

    /// <summary>Changed only when source fields change.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Copies the source fields; returns true when something changed.</summary>
    public bool Apply(MessageSource source, DateTimeOffset now)
    {
        var changed = MessageAt != source.MessageAt || Text != source.Text || StatusOriginal != source.StatusOriginal
                      || MarkAt != source.MarkAt || MarkBy != source.MarkBy || PaymentState != source.PaymentState
                      || MessageDate != source.MessageDate;
        if (!changed) return false;
        MessageAt = source.MessageAt;
        MessageDate = source.MessageDate;
        Text = source.Text;
        StatusOriginal = source.StatusOriginal;
        MarkAt = source.MarkAt;
        MarkBy = source.MarkBy;
        PaymentState = source.PaymentState;
        UpdatedAt = now;
        return true;
    }
}
