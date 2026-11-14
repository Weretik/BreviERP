using Content.Domain.ProductPages;

namespace Content.Application.Features.ProductPages.Shared;

public sealed record ContentProductVariantResponse(
    long ArticleId,
    string SaleSku,
    bool IsActive,
    string? FabricName,
    string? FabricDetails,
    ContentPriceTiersResponse? Prices);
