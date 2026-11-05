namespace Purchasing.Domain.Messages;

/// <summary>The manual supplier link and note of a message at one version.</summary>
public sealed record PurchaseReview(int? SupplierId, string? Note, int Version);

/// <summary>
/// History of the manual supplier link and note: one entry per saved change, never edited. It does not replace the
/// revisions of the sheet rows.
/// </summary>
public sealed class PurchaseReviewChange
{
    private PurchaseReviewChange() { }

    internal PurchaseReviewChange(string messageId, string reviewer, DateTimeOffset at, PurchaseReview before,
        PurchaseReview after)
    {
        MessageId = messageId;
        Reviewer = reviewer;
        At = at;
        Before = before;
        After = after;
    }

    public long Id { get; private set; }
    public string MessageId { get; private set; } = null!;

    /// <summary>E-mail of the staff account that saved the change.</summary>
    public string Reviewer { get; private set; } = null!;

    public DateTimeOffset At { get; private set; }
    public PurchaseReview Before { get; private set; } = null!;
    public PurchaseReview After { get; private set; } = null!;
}
