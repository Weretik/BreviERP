namespace Crm.Application.Documents.Artifacts;

public sealed record QuotePdfArtifactContent(QuotePdfArtifactMetadata Artifact, byte[] Bytes);
