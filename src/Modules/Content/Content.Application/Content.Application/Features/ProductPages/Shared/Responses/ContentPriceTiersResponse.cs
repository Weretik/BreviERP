using Content.Domain.ProductPages;

namespace Content.Application.Features.ProductPages.Shared.Responses;

public sealed record ContentPriceTiersResponse(decimal Retail, decimal Wholesale, decimal LargeWholesale);
