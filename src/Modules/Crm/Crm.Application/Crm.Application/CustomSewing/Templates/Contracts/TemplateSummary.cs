namespace Crm.Application.CustomSewing.Templates.Contracts;

public sealed record TemplateSummary(Guid PublicId, int OwnerId, long Revision, bool Archived,
    string Kind, int VersionNumber, string Name, string ProductType, string Season, DateTimeOffset UpdatedAt);
