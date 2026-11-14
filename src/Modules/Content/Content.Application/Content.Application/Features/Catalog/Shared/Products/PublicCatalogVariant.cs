using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.ProductPages.Shared.Responses;

namespace Content.Application.Features.Catalog.Shared.Products;

/// <summary>A fabric of a sewing model with its three tier prices; null prices mean «ціну уточнюйте».</summary>
public sealed record PublicCatalogVariant(
    string Sku,
    string? FabricName,
    string? FabricDetails,
    ContentPriceTiersResponse? Prices);
