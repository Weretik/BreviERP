using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;

namespace Crm.Application.Quotes.Pricing;

public sealed record QuoteCustomVersionSnapshot(Guid DraftPublicId, Guid VersionPublicId, int Number,
    CustomConfiguration Configuration, CustomPriceBasis Basis);
