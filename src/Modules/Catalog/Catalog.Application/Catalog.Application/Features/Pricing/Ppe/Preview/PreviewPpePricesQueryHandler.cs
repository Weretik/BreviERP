using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Pricing.Ppe;

namespace Catalog.Application.Features.Pricing.Ppe.Preview;

public sealed class PreviewPpePricesQueryHandler(IPpePricingInputReader inputs)
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
