using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Features.Product.Shared.Sewing;

namespace SewingPricing.Jobs.Activation;

public sealed record ActivationArticlePlan(long ArticleId, string SaleSku, IReadOnlyList<SewingMaterialInput> Materials,
    long? SettingsVersionId, IReadOnlyList<long> ReferenceVersionIds, SewingPriceTiers ExpectedPrices, Guid SaveRequestId);
