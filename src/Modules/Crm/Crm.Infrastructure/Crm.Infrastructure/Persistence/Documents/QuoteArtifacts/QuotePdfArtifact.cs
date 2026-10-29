using System.Security.Cryptography;
using Crm.Application.Documents.Artifacts;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Infrastructure.Persistence.Documents.Artifacts;

// Private PDF bytes belong to the CRM database; no public file path or URL is persisted.
public sealed class QuotePdfArtifact
{
    public const int MaximumPdfBytes = 20 * 1024 * 1024;
    public long Id { get; private set; }
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public QuoteId QuoteId { get; private set; }
    public long QuoteVersionId { get; private set; }
    public IssuerSettingsVersionId IssuerVersionId { get; private set; }
    public DocumentTemplateVersionId TemplateVersionId { get; private set; }
    public string RenderKey { get; private set; } = null!;
    public string Status { get; private set; } = "queued";
    public int Attempt { get; private set; }
    public Guid? LeaseToken { get; private set; }
    public DateTimeOffset? LeaseUntil { get; private set; }
    public byte[]? PdfBytes { get; private set; }
    public string? Sha256 { get; private set; }
    public long? Length { get; private set; }
    public string? ErrorCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? GeneratedAt { get; private set; }

    private QuotePdfArtifact() { }

    public static QuotePdfArtifact Queue(QuoteId quoteId, long quoteVersionId,
        IssuerSettingsVersionId issuerVersionId, DocumentTemplateVersionId templateVersionId,
        string renderKey, DateTimeOffset now)
    {
        if (quoteId.Value <= 0 || quoteVersionId <= 0 || issuerVersionId.Value <= 0 ||
            templateVersionId.Value <= 0 || string.IsNullOrWhiteSpace(renderKey) || renderKey.Length > 128)
            throw new ArgumentException("Invalid pinned PDF identity.", nameof(renderKey));
        return new QuotePdfArtifact
        {
            QuoteId = quoteId, QuoteVersionId = quoteVersionId,
            IssuerVersionId = issuerVersionId, TemplateVersionId = templateVersionId,
            RenderKey = renderKey, CreatedAt = now, UpdatedAt = now
        };
    }

    public Guid Claim(DateTimeOffset now, TimeSpan lease)
    {
        if (lease <= TimeSpan.Zero || Status is not "queued" &&
            !(Status == "processing" && LeaseUntil <= now))
            throw new InvalidOperationException("PDF artifact is not claimable.");
        var token = Guid.NewGuid();
        Status = "processing";
        Attempt = checked(Attempt + 1);
        LeaseToken = token;
        LeaseUntil = now.Add(lease);
        ErrorCode = null;
        UpdatedAt = now;
        return token;
    }

    public void Complete(Guid leaseToken, byte[] bytes, DateTimeOffset now)
    {
        AssertLease(leaseToken, now);
        if (bytes is null || bytes.Length is < 8 or > MaximumPdfBytes ||
            !bytes.AsSpan().StartsWith("%PDF-"u8))
            throw new ArgumentException("Invalid or oversized PDF bytes.", nameof(bytes));
        PdfBytes = bytes.ToArray();
        Sha256 = Convert.ToHexString(SHA256.HashData(PdfBytes));
        Length = PdfBytes.Length;
        Status = "succeeded";
        LeaseToken = null;
        LeaseUntil = null;
        GeneratedAt = now;
        UpdatedAt = now;
    }

    public void Fail(Guid leaseToken, string errorCode, DateTimeOffset now)
    {
        AssertLease(leaseToken, now);
        if (string.IsNullOrWhiteSpace(errorCode) || errorCode.Length > 80 ||
            errorCode.Any(c => c is not (>= 'a' and <= 'z') and not (>= '0' and <= '9') and not '_'))
            throw new ArgumentException("Invalid PDF error code.", nameof(errorCode));
        Status = "failed";
        ErrorCode = errorCode;
        LeaseToken = null;
        LeaseUntil = null;
        UpdatedAt = now;
    }

    public void Retry(DateTimeOffset now)
    {
        if (Status != "failed" || PdfBytes is not null)
            throw new InvalidOperationException("Only failed PDF artifacts can be retried.");
        Status = "queued";
        ErrorCode = null;
        UpdatedAt = now;
    }

    public bool HasIntactContent() => Status == "succeeded" && PdfBytes is { Length: > 0 } &&
        Length == PdfBytes.LongLength && Sha256 == Convert.ToHexString(SHA256.HashData(PdfBytes));

    public QuotePdfArtifactMetadata Metadata() => new(PublicId, QuoteId, QuoteVersionId,
        IssuerVersionId, TemplateVersionId, RenderKey, Status, Attempt, Sha256,
        Length, ErrorCode, CreatedAt, UpdatedAt, GeneratedAt);

    private void AssertLease(Guid leaseToken, DateTimeOffset now)
    {
        if (leaseToken == Guid.Empty || Status != "processing" || LeaseToken != leaseToken || LeaseUntil <= now)
            throw new InvalidOperationException("PDF artifact lease is no longer current.");
    }
}
