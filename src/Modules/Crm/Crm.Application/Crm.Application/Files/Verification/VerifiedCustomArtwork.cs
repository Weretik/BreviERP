namespace Crm.Application.Files.Verification;

public sealed record VerifiedCustomArtwork(Guid DraftPublicId, IReadOnlyList<Guid> FileIds);
