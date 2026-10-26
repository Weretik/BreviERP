using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Contracts.Pricing.Ppe;
public sealed record PpeProductPricing(int ProductId, bool IsEnrolled, PpeOfferVersion? Offer, PpeRuleVersion? Rule, PpePriceCalculation Calculation);
