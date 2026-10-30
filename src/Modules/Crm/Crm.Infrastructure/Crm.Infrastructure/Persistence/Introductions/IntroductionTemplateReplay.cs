using Crm.Application.Quotes.Introductions;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Infrastructure.Persistence.Introductions;

internal sealed record IntroductionTemplateReplay(
    Guid TemplatePublicId, int OwnerId, long Revision, bool Archived,
    Guid VersionPublicId, int VersionNumber, string Name,
    IntroductionTemplatePayload Payload, DateTimeOffset UpdatedAt)
{
    public static IntroductionTemplateReplay FromDetail(IntroductionTemplateDetail detail,
        IntroductionTemplatePayload payload) => new(detail.TemplatePublicId, detail.OwnerId,
        detail.Revision, detail.Archived, detail.VersionPublicId, detail.VersionNumber,
        detail.Name, payload, detail.UpdatedAt);

    public IntroductionTemplateDetail ToDetail() => new(TemplatePublicId, OwnerId, Revision,
        Archived, VersionPublicId, VersionNumber, Name, Payload.ToSnapshot(), UpdatedAt);
}
