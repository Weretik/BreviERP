using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;

/// <summary>
/// The live price of the «Розрахунок» form. It is computed here, not in the browser, so a Manager never receives the
/// rates or the cost (R-02).
/// </summary>
public sealed record CalculateEmbroideryQuery(EmbroideryActor Actor, CalculateEmbroideryInput Input)
    : IQuery<Result<EmbroideryCalculationResponse>>;
