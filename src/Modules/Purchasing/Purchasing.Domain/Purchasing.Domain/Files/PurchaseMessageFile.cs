using Purchasing.Domain.Messages.Identifiers;
using BuildingBlocks.Domain.Entity;
namespace Purchasing.Domain.Files;

/// <summary>
/// A link from a message to an original document (usually Dropbox). Only the link is stored, not the file. A changed URL
/// is a different file; a link removed from the sheet becomes inactive and is kept.
/// </summary>
public sealed class PurchaseMessageFile : IAggregateRoot
{
    public const string AwaitingRecognition = "awaiting_recognition";

    private PurchaseMessageFile() { }

    private PurchaseMessageFile(string id, PurchaseMessageId messageId, string url, Guid run)
    {
        Id = new PurchaseMessageFileId(id);
        MessageId = messageId;
        Url = url;
        Active = true;
        SeenRun = run;
        State = AwaitingRecognition;
    }

    public static PurchaseMessageFile Create(string id, PurchaseMessageId messageId, string url, Guid run) =>
        new(id, messageId, url, run);

    /// <summary>SHA-256 of messageId:exactUrl, lower-case hex.</summary>
    public PurchaseMessageFileId Id { get; private set; }

    public PurchaseMessageId MessageId { get; private set; }

    /// <summary>The exact link from the sheet, never rewritten or canonicalised.</summary>
    public string Url { get; private set; } = null!;

    public bool Active { get; private set; }
    public Guid SeenRun { get; private set; }

    /// <summary>Stored file state; the card's state is computed separately.</summary>
    public string State { get; private set; } = null!;

    public string? Issue { get; private set; }

    /// <summary>Marks the link as seen in this run; returns true when it was inactive before.</summary>
    public bool See(Guid run)
    {
        SeenRun = run;
        if (Active) return false;
        Active = true;
        return true;
    }

    public bool Deactivate()
    {
        if (!Active) return false;
        Active = false;
        return true;
    }
}
