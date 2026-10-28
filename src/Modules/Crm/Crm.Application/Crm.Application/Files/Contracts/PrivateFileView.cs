namespace Crm.Application.Files.Contracts;

public sealed record PrivateFileView(Guid PublicId, Guid DraftId, string ContentType, long Length,
    string Sha256, string State, DateTimeOffset ExpiresAt, bool ShowInQuote, bool Production, bool IsFinalized);
