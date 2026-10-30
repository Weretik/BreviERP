using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.Introductions;

public sealed record IntroductionTemplateDetail(
    Guid TemplatePublicId, int OwnerId, long Revision, bool Archived,
    Guid VersionPublicId, int VersionNumber, string Name,
    IntroductionSelectionSnapshot Snapshot, DateTimeOffset UpdatedAt);
