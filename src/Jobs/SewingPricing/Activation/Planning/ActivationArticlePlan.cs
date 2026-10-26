using Catalog.Application.Features.Product.Sewing;
using Catalog.Domain.Pricing.Sewing;

namespace SewingPricing.Jobs.Activation;

public sealed record ActivationArticlePlan(long ArticleId, string SaleSku, IReadOnlyList<SewingMaterialInput> Materials,
    long? SettingsVersionId, IReadOnlyList<long> ReferenceVersionIds, SewingPriceTiers ExpectedPrices, Guid SaveRequestId);
