namespace Crm.Application.Quotes.Contracts.Responses;

public sealed record QuoteSummary(
    Guid PublicId, string RecipientName, long Revision, int VersionNumber,
    string State, bool Archived, string? Number, int OwnerId, DateTimeOffset UpdatedAt);
