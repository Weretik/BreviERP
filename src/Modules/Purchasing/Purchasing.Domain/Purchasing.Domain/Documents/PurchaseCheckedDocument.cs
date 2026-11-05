namespace Purchasing.Domain.Documents;

/// <summary>
/// Checked facts of one original file (one per file id): read by AI and passed the arithmetic check, or uploaded by hand.
/// A different version replaces it only on explicit request; the old one goes to <see cref="PurchaseCheckedDocumentHistory"/>.
/// </summary>
public sealed class PurchaseCheckedDocument
{
    public const int ByMaxLength = 256;

    private PurchaseCheckedDocument() { }

    public PurchaseCheckedDocument(string fileId, string messageId, PurchaseDocument document, PurchaseDocumentOrigin origin,
        string? model, decimal? costUsd, string checkedBy, DateTimeOffset checkedAt)
    {
        Id = fileId;
        MessageId = messageId;
        Set(document, origin, model, costUsd, checkedBy, checkedAt);
    }

    /// <summary>The file id (SHA-256 of messageId:url).</summary>
    public string Id { get; private set; } = null!;

    public string MessageId { get; private set; } = null!;
    public string SourceUid { get; private set; } = null!;
    public string OriginalUrl { get; private set; } = null!;

    /// <summary>Canonical JSON (<see cref="PurchaseDocumentJson"/>); the source of truth for every shown value.</summary>
    public string PayloadJson { get; private set; } = null!;

    public string PayloadHash { get; private set; } = null!;
    public PurchaseDocumentOrigin Origin { get; private set; }

    /// <summary>OpenAI model that read it; null for a manual file.</summary>
    public string? Model { get; private set; }

    public decimal? CostUsd { get; private set; }

    /// <summary>E-mail of who pressed «Розпізнати» or uploaded the file.</summary>
    public string CheckedBy { get; private set; } = null!;

    public DateTimeOffset CheckedAt { get; private set; }

    /// <summary>Moves the current version to history and stores the new one.</summary>
    public PurchaseCheckedDocumentHistory Replace(PurchaseDocument document, PurchaseDocumentOrigin origin, string? model,
        decimal? costUsd, string by, DateTimeOffset now)
    {
        var history = new PurchaseCheckedDocumentHistory(Id, PayloadJson, PayloadHash, Origin, CheckedBy, CheckedAt, by, now);
        Set(document, origin, model, costUsd, by, now);
        return history;
    }

    private void Set(PurchaseDocument document, PurchaseDocumentOrigin origin, string? model, decimal? costUsd, string by,
        DateTimeOffset at)
    {
        SourceUid = document.SourceUid;
        OriginalUrl = document.OriginalFileLink;
        PayloadJson = PurchaseDocumentJson.Write(document);
        PayloadHash = PurchaseDocumentJson.Hash(PayloadJson);
        Origin = origin;
        Model = model;
        CostUsd = costUsd;
        CheckedBy = by.Length > ByMaxLength ? by[..ByMaxLength] : by;
        CheckedAt = at;
    }
}

public enum PurchaseDocumentOrigin
{
    Ai,
    Manual,
}

/// <summary>A replaced version of a checked document; append-only.</summary>
public sealed class PurchaseCheckedDocumentHistory
{
    private PurchaseCheckedDocumentHistory() { }

    internal PurchaseCheckedDocumentHistory(string fileId, string payloadJson, string payloadHash, PurchaseDocumentOrigin origin,
        string checkedBy, DateTimeOffset checkedAt, string replacedBy, DateTimeOffset replacedAt)
    {
        FileId = fileId;
        PayloadJson = payloadJson;
        PayloadHash = payloadHash;
        Origin = origin;
        CheckedBy = checkedBy;
        CheckedAt = checkedAt;
        ReplacedBy = replacedBy.Length > PurchaseCheckedDocument.ByMaxLength ? replacedBy[..PurchaseCheckedDocument.ByMaxLength] : replacedBy;
        ReplacedAt = replacedAt;
    }

    public long Id { get; private set; }
    public string FileId { get; private set; } = null!;
    public string PayloadJson { get; private set; } = null!;
    public string PayloadHash { get; private set; } = null!;
    public PurchaseDocumentOrigin Origin { get; private set; }
    public string CheckedBy { get; private set; } = null!;
    public DateTimeOffset CheckedAt { get; private set; }
    public string ReplacedBy { get; private set; } = null!;
    public DateTimeOffset ReplacedAt { get; private set; }
}
