using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Pricing.Ppe;

namespace Catalog.Application.Features.Pricing.Ppe.Preview;

public sealed class PreviewPpePricesQueryHandler(IPpePricingInputReader inputs)
    : IQueryHandler<PreviewPpePricesQuery, Result<PpePriceCalculation>>
{
    public async ValueTask<Result<PpePriceCalculation>> Handle(PreviewPpePricesQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.RuleCode))
            return Result.Invalid(new ValidationError(nameof(query.RuleCode), "Оберіть правило націнки."));
        if (query.SupplierPrice is <= 0 or >= 10_000_000)
            return Result.Invalid(new ValidationError(nameof(query.SupplierPrice), "Ціна постачальника має бути більша за 0."));

        var rule = await inputs.ReadRuleAsync(query.RuleCode.Trim(), cancellationToken);
        if (rule is null)
            return Result.Invalid(new ValidationError(nameof(query.RuleCode), "Правило ціноутворення не знайдено."));

        return Result.Success(PpeCalculator.Calculate(new PpePricingInput(query.SupplierPrice, true, null, null, true,
            rule.IsApproved ? [rule.Values.Markups] : [])));
    }
}
