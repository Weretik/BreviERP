using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;

namespace Purchasing.Domain.Messages;

/// <summary>
/// One Telegram message of the purchase sheet; several sheet rows (UIDs) may belong to it. Source fields are refreshed by
/// every sync; the supplier link, the note and their version are Brevi's own and the sync never touches them.
/// </summary>
public sealed class PurchaseMessage : IAggregateRoot
{
    public const int ReviewNoteMaxLength = 1_000;
    public const int ReviewerMaxLength = 256;

    private readonly List<PurchasePayment> _payments = [];

    private PurchaseMessage() { }

    private PurchaseMessage(string id, string telegramKey, MessageSource source, DateTimeOffset now)
    {
        Id = id;
        TelegramKey = telegramKey;
        ReviewVersion = 0;
        Apply(source, now);
    }

    public static PurchaseMessage Create(string id, string telegramKey, MessageSource source, DateTimeOffset now) =>
        new(id, telegramKey, source, now);

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

    /// <summary>Optimistic version of the manual supplier link, note and payment links; starts at 0.</summary>
    public int ReviewVersion { get; private set; }

    /// <summary>Who last changed the supplier link, note or payments (e-mail of the staff account); null before the first change.</summary>
    public string? ReviewedBy { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

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

    /// <summary>The payment links of this invoice; the commands that link or unlink load the active ones.</summary>
    public IReadOnlyList<PurchasePayment> Payments => _payments;

    /// <summary>
    /// Links an expense of «Бухгалтерія» as a payment of this invoice and adds 1 to the review version. The caller checks
    /// the expected version and that the operation is an active expense.
    /// </summary>
    public DomainResult LinkPayment(Guid transactionId, string by, DateTimeOffset now)
    {
        if (_payments.Any(x => x.TransactionId == transactionId && x.Active))
            return PurchaseMessageErrors.PaymentAlreadyLinked();
        _payments.Add(new PurchasePayment(Id, transactionId, by, now));
        Reviewed(by, now);
        return DomainResult.Success();
    }

    /// <summary>Removes the active link of an expense to this invoice and adds 1 to the review version.</summary>
    public DomainResult UnlinkPayment(Guid transactionId, string by, DateTimeOffset now)
    {
        var payment = _payments.FirstOrDefault(x => x.TransactionId == transactionId && x.Active);
        if (payment is null) return PurchaseMessageErrors.PaymentNotLinked();
        payment.Unlink(by, now);
        Reviewed(by, now);
        return DomainResult.Success();
    }

    private void Reviewed(string by, DateTimeOffset now)
    {
        ReviewVersion++;
        ReviewedBy = by;
        ReviewedAt = now;
    }

    /// <summary>
    /// Sets the supplier link and note by hand. The note is trimmed and an empty one is removed. Returns the history entry,
    /// or null when nothing changed (the version stays). The caller checks the expected version first.
    /// </summary>
    public DomainResult<PurchaseReviewChange?> Review(int? supplierId, string? note, string reviewer, DateTimeOffset now)
    {
        var cleanNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (cleanNote is { Length: > ReviewNoteMaxLength })
            return PurchaseMessageErrors.NoteTooLong();
        if (supplierId == SupplierId && cleanNote == ReviewNote) return DomainResult<PurchaseReviewChange?>.Success(null);

        var change = new PurchaseReviewChange(Id, reviewer, now,
            new PurchaseReview(SupplierId, ReviewNote, ReviewVersion),
            new PurchaseReview(supplierId, cleanNote, ReviewVersion + 1));
        SupplierId = supplierId;
        ReviewNote = cleanNote;
        Reviewed(reviewer, now);
        return (PurchaseReviewChange?)change;
    }
}
