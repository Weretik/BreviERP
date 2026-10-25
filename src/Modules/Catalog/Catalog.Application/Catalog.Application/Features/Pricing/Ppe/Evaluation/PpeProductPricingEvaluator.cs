using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Features.Pricing.Ppe.Evaluation;
public static class PpeProductPricingEvaluator
{
    public static PpeProductPricing Evaluate(PpePricingProductInput input)
    {
        PpePriceCalculation calculation;
        if (!input.IsActive) calculation = new(null, [new("InactiveProduct", "product")]);
        else if (input.Offer is not { } offer) calculation = new(null, [new("MissingOffer", "offer")]);
        else calculation = PpeCalculator.Calculate(new(offer.Values.SupplierCost, offer.IsApproved,
            offer.Values.ExternalRetail, offer.Values.ExternalWholesale, offer.IsApproved,
            input.Rule is { IsApproved: true } rule ? [rule.Values.Markups] : []));
        return new(input.ProductId, input.Offer is not null, input.Offer, input.Rule, calculation);
    }
}
