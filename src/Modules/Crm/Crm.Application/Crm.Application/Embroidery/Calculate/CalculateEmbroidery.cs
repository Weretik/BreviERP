using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;

namespace Crm.Application.Embroidery.Calculate;

/// <summary>
/// The live price of the «Розрахунок» form. It is computed here, not in the browser, so a Manager never receives the
/// rates or the cost (R-02).
/// </summary>
public sealed record CalculateEmbroideryQuery(EmbroideryActor Actor, CalculateEmbroideryInput Input)
    : IQuery<Result<EmbroideryCalculationResponse>>;

public sealed class CalculateEmbroideryQueryHandler(IEmbroideryStore store, IEmbroideryStitchRateReader stitchRate)
    : IQueryHandler<CalculateEmbroideryQuery, Result<EmbroideryCalculationResponse>>
{
    public async ValueTask<Result<EmbroideryCalculationResponse>> Handle(CalculateEmbroideryQuery query,
        CancellationToken cancellationToken)
    {
        var priced = await EmbroideryPricing.PriceAsync(store, stitchRate, query.Input.ToDomain(), cancellationToken);
        return Result.Success(priced.Calculation.Result is { } result
            ? result.ToResponse(query.Actor.SeesCost)
            : EmbroideryCalculationMapper.Failed(priced.Calculation.Error!));
    }
}
