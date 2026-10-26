using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Pricing.Ppe;

namespace SewingPricing.Jobs.Activation;

public sealed record ActivationOfferPlan(int ProductId, string SaleSku, Guid ExpectedId, Guid RequestId, PpeOfferValues Values, PpePriceTiers ExpectedPrices);
