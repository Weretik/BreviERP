using Crm.Domain.CustomSewing.Attachments;

namespace Crm.Application.Files.Verification;

public sealed record PrivateFileVerification(Guid FileId, string ContentType, long Length, string Sha256,
    CustomFileState State, DateTimeOffset ExpiresAt, bool Linked, bool Retained);
