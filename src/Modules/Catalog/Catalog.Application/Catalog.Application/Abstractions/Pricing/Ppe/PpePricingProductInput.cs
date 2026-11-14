using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Application.Abstractions.Pricing.Ppe;
public sealed record PpePricingProductInput(int ProductId, bool IsActive, PpeOfferVersion? Offer, PpeRuleVersion? Rule);
