namespace Crm.Application.Quotes.Introductions;

public sealed record IntroductionTemplateSummary(
    Guid TemplatePublicId, int OwnerId, long Revision, bool Archived,
    Guid VersionPublicId, int VersionNumber, string Name, DateTimeOffset UpdatedAt);
