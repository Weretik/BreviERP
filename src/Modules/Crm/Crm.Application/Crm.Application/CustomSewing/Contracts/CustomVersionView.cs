using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;

namespace Crm.Application.CustomSewing.Contracts;

public sealed record CustomVersionView(Guid PublicId, int Number, Guid? ParentVersionId,
    CustomConfiguration Configuration, CustomPriceBasis Basis, int ActorId, DateTimeOffset CreatedAt);
