namespace Crm.Application.Documents.Artifacts;

// LeaseToken stays internal to the rendering worker, never in the public API response.
public sealed record QuotePdfArtifactClaim(QuotePdfArtifactMetadata Artifact, Guid LeaseToken);
