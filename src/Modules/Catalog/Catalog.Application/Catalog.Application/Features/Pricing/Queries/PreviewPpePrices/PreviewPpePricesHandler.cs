using Catalog.Domain.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Queries.PreviewPpePrices;

public sealed class PreviewPpePricesHandler(IPpePricingInputReader inputs)
    : IQueryHandler<PreviewPpePricesQuery, Result<PpePriceCalculation>>
{
    public async ValueTask<Result<PpePriceCalculation>> Handle(PreviewPpePricesQuery query, CancellationToken cancellationToken)
    {
        var rule = await inputs.ReadRuleAsync(query.RuleCode.Trim(), cancellationToken);
        if (rule is null)
            return Result.Invalid(new ValidationError(nameof(query.RuleCode), "Правило ціноутворення не знайдено."));

        return Result.Success(PpeCalculator.Calculate(new PpePricingInput(query.SupplierPrice, true, null, null, true,
            rule.IsApproved ? [rule.Values.Markups] : [])));
    }
}
