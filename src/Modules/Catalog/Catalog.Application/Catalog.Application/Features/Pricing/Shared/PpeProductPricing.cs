using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Application.Features.Pricing.Shared;
public sealed record PpeProductPricing(int ProductId, bool IsEnrolled, PpeOfferVersion? Offer, PpeRuleVersion? Rule, PpePriceCalculation Calculation);
