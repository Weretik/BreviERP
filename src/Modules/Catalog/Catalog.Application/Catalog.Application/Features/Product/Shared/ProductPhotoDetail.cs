namespace Catalog.Application.Features.Product.Shared;

public sealed record ProductPhotoDetail(
    int MediaFileId,
    string Url,
    string? Alt,
    bool IsVisible,
    bool IsMain,
    int SortOrder);
