using System.Security.Cryptography;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Infrastructure.Persistence.Documents.Production;

public sealed class ProductionPackageArtifact
{
    public const int MaximumBytes = 64 * 1024 * 1024;
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public QuoteId QuoteId { get; set; }
    public long QuoteVersionId { get; set; }
    public string SelectionHash { get; set; } = null!;
    public int SchemaVersion { get; set; }
    public string Sha256 { get; set; } = null!;
    public long Length { get; set; }
    public byte[] Bytes { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    internal bool IsIntact() => Bytes is { Length: > 0 and <= MaximumBytes } &&
        Bytes.LongLength == Length && Convert.ToHexString(SHA256.HashData(Bytes)) == Sha256;
}
