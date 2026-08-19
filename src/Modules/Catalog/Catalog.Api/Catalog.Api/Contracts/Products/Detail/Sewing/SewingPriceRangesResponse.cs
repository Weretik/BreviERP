namespace Catalog.Api.Contracts.Products;

public sealed record SewingPriceRangesResponse(SewingPriceRangeResponse Price1To10, SewingPriceRangeResponse Price11To39, SewingPriceRangeResponse Price40Plus);
