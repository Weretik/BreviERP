namespace Crm.Application.Quotes.Contracts.Responses;

public sealed record QuoteDetail(
    Guid PublicId, Guid? CustomerPublicId, Guid? ContactPublicId, string RecipientName,
    long Revision, string State, bool Archived, string? Number, int OwnerId,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, bool CanEdit, QuoteVersionView Version,
    Guid? SourceQuoteId = null, Guid? SourceVersionId = null);
