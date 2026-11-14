using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;

public sealed record ProductOverviewReadModel(
    int Id,
    ProductType Type,
    string? ModelCode,
    string? SaleSku,
    string Name,
    int? SectionId,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    int? MainPhotoFileId = null);
