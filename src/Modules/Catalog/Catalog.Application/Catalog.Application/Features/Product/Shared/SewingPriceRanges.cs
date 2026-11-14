namespace Catalog.Application.Features.Product.Shared;

public sealed record SewingPriceRanges(
    SewingPriceRange Price1To10,
    SewingPriceRange Price11To39,
    SewingPriceRange Price40Plus);
