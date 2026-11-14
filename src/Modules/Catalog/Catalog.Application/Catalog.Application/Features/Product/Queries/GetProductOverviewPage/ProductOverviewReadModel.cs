using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewPage;

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
