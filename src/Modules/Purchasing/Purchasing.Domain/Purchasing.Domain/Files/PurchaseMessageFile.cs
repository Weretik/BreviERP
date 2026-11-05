namespace Purchasing.Domain.Files;

/// <summary>
/// A link from a message to an original document (usually Dropbox). Only the link is stored, not the file. A changed URL
/// is a different file; a link removed from the sheet becomes inactive and is kept.
/// </summary>
public sealed class PurchaseMessageFile
{
    public const string AwaitingRecognition = "awaiting_recognition";

    private PurchaseMessageFile() { }

    public PurchaseMessageFile(string id, string messageId, string url, Guid run)
    {
        Id = id;
        MessageId = messageId;
        Url = url;
        Active = true;
        SeenRun = run;
        State = AwaitingRecognition;
    }

    /// <summary>SHA-256 of messageId:exactUrl, lower-case hex.</summary>
    public string Id { get; private set; } = null!;

    public string MessageId { get; private set; } = null!;

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
