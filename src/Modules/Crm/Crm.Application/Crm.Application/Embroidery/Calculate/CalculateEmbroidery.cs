using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;
using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Settings;

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
        var rate = await stitchRate.ReadAsync(cancellationToken);
        if (rate.Value is not { } stitch || stitch < 0)
            return Result.Success(EmbroideryCalculationMapper.Failed(EmbroideryMessages.StitchRateMissing));

        var saved = await store.FindSettingsAsync(tracking: false, cancellationToken);
        var calculation = EmbroideryCalculator.Calculate(query.Input.ToDomain(),
            saved?.Content ?? EmbroiderySettingsDefaults.Content, stitch);
        return Result.Success(calculation.Result is { } result
            ? result.ToResponse(query.Actor.SeesCost)
            : EmbroideryCalculationMapper.Failed(calculation.Error!));
    }
}
