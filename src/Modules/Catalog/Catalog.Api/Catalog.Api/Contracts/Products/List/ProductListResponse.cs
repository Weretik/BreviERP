namespace Catalog.Api.Contracts.Products;

public sealed record ProductListResponse(
    int Id,
    string Name,
    string Slug,
    string Type,
    IReadOnlyList<int> CategoryIds,
    ProductMainPhotoResponse? MainPhoto,
    decimal MinimumWholesalePrice,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
