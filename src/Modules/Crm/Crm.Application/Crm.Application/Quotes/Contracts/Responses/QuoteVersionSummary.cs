namespace Crm.Application.Quotes.Contracts.Responses;

public sealed record QuoteVersionSummary(Guid PublicId, int VersionNumber, string Action,
    int ActorId, DateTimeOffset CreatedAt, Guid? ParentVersionId);
