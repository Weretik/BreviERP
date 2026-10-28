namespace Crm.Application.Quotes.Contracts.Responses;

public sealed record QuoteEventView(Guid PublicId, long Revision, int VersionNumber, string Action,
    int ActorId, int OwnerId, int? PreviousOwnerId, string? Reason, DateTimeOffset CreatedAt);
