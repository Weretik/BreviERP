using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Contracts.Pricing.Ppe;
public sealed record PpePricingProductInput(int ProductId, bool IsActive, PpeOfferVersion? Offer, PpeRuleVersion? Rule);
