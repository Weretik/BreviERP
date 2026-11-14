using Catalog.Domain.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing;

namespace SewingPricing.Jobs.Activation;

public sealed record ActivationOfferPlan(int ProductId, string SaleSku, Guid ExpectedId, Guid RequestId, PpeOfferValues Values, PpePriceTiers ExpectedPrices);
