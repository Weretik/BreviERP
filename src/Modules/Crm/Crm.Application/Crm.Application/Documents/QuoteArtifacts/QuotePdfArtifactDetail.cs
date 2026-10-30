namespace Crm.Application.Documents.Artifacts;

// Public API state; internal database IDs and lease token never cross the boundary.
public sealed record QuotePdfArtifactDetail(Guid ArtifactId, string Status, string? Sha256,
    long? Length, string? ErrorCode, DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt, DateTimeOffset? GeneratedAt)
{
    public static QuotePdfArtifactDetail From(QuotePdfArtifactMetadata metadata) =>
        new(metadata.PublicId, metadata.Status, metadata.Sha256, metadata.Length,
            metadata.ErrorCode, metadata.CreatedAt, metadata.UpdatedAt, metadata.GeneratedAt);
}
