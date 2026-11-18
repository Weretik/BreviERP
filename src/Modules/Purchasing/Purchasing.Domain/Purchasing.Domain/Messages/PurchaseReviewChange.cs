using BuildingBlocks.Domain.Identifiers.ReferenceData;
using Purchasing.Domain.Messages.Identifiers;
using BuildingBlocks.Domain.Entity;
namespace Purchasing.Domain.Messages;

/// <summary>The manual supplier link and note of a message at one version.</summary>
public sealed record PurchaseReview(SupplierId? SupplierId, string? Note, int Version);

/// <summary>
/// History of the manual supplier link and note: one entry per saved change, never edited. It does not replace the
/// revisions of the sheet rows.
/// </summary>
public sealed class PurchaseReviewChange : IAggregateRoot
{
    private PurchaseReviewChange() { }

    internal PurchaseReviewChange(PurchaseMessageId messageId, string reviewer, DateTimeOffset at, PurchaseReview before,
        PurchaseReview after)
    {
        MessageId = messageId;
        Reviewer = reviewer;
        At = at;
        Before = before;
        After = after;
    }

    public PurchaseReviewChangeId Id { get; private set; }
    public PurchaseMessageId MessageId { get; private set; }

    /// <summary>E-mail of the staff account that saved the change.</summary>
    public string Reviewer { get; private set; } = null!;

    public DateTimeOffset At { get; private set; }
    public PurchaseReview Before { get; private set; } = null!;
    public PurchaseReview After { get; private set; } = null!;
}
