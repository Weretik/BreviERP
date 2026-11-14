using Accounting.Domain;
using Accounting.Domain.Transactions;

namespace Accounting.Domain.Attachments;

/// <summary>
/// A file of an income or expense operation (a receipt, an invoice, an act): a PDF, JPEG or PNG up to 10 MiB in the
/// private storage of «Бухгалтерія». A removed file keeps its row with <see cref="DeletedAt"/>, so «Історія» still
/// names it.
/// </summary>
public sealed class FinanceAttachment : IAggregateRoot
{
    public const int FileNameMaxLength = 255;
    public const int ContentTypeMaxLength = 64;
    public const int StorageKeyMaxLength = 128;
    public const int ByMaxLength = 256;
    public const long MaxBytes = 10 * 1024 * 1024;

    private FinanceAttachment() { }

    public FinanceAttachmentId Id { get; private set; }
    public FinanceTransactionId TransactionId { get; private set; }

    /// <summary>The name the file had on the computer it came from; only shown.</summary>
    public string FileName { get; private set; } = null!;

    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }

    /// <summary>SHA-256 of the content, lowercase hex.</summary>
    public string Sha256 { get; private set; } = null!;

    /// <summary><c>finance/&lt;transactionId&gt;/&lt;attachmentId&gt;.&lt;ext&gt;</c>.</summary>
    public string StorageKey { get; private set; } = null!;

    public string CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public static DomainResult<FinanceAttachment> Create(FinanceAttachmentId id, FinanceTransactionId transactionId, string fileName,
        FinanceAttachmentKind kind, long sizeBytes, string sha256, string by, DateTimeOffset now)
    {
        if (sizeBytes is <= 0 or > MaxBytes)
            return AccountingErrors.Rule("attachment.size", "Файл має бути від 1 байта до 10 МБ.");
        var name = fileName.Trim();
        if (name.Length is 0 or > FileNameMaxLength)
            return AccountingErrors.Rule("attachment.name", "Назва файлу обовʼязкова, до 255 символів.");
        return new FinanceAttachment
        {
            Id = id,
            TransactionId = transactionId,
            FileName = name,
            ContentType = kind.ContentType,
            SizeBytes = sizeBytes,
            Sha256 = sha256,
            StorageKey = KeyOf(transactionId, id, kind),
            CreatedBy = by,
            CreatedAt = now,
        };
    }

    public static string KeyOf(FinanceTransactionId transactionId, FinanceAttachmentId id, FinanceAttachmentKind kind) =>
        $"finance/{transactionId.Value:D}/{id.Value:D}{kind.Extension}";

    public void Delete(string by, DateTimeOffset now)
    {
        if (DeletedAt is not null) return;
        DeletedAt = now;
        DeletedBy = by;
    }
}
