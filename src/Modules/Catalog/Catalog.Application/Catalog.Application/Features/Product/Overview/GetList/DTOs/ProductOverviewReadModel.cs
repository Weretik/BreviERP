using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Overview.GetList.DTOs;

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
    DateTimeOffset? UpdatedAt);
