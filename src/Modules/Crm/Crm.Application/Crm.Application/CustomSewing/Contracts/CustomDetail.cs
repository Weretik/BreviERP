using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;
using Crm.Domain.CustomSewing.Drafts;

namespace Crm.Application.CustomSewing.Contracts;

public sealed record CustomDetail(Guid PublicId, long Revision, int OwnerId, Guid? CustomerPublicId,
    Guid? QuotePublicId, CustomConfiguration Configuration, CustomVersionView? CurrentVersion,
    CustomPriceBasis? Preview, DateTimeOffset UpdatedAt, bool CanEdit, CustomTemplateOrigin? TemplateOrigin = null);
