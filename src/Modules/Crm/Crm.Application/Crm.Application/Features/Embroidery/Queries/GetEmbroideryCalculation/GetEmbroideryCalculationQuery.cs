using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculation;

/// <summary>One saved calculation as it was saved; a Manager gets no cost and no rates.</summary>
public sealed record GetEmbroideryCalculationQuery(EmbroideryActor Actor, long Id)
    : IQuery<Result<EmbroiderySavedCalculationResponse>>;
