using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;

public sealed class CalculateEmbroideryHandler(IEmbroideryStore store, IEmbroideryStitchRateReader stitchRate)
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
