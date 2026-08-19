namespace Catalog.Application.Contracts.Admin.Product;

public sealed record ProductPhotoDetail(
    int MediaFileId,
    string Url,
    string? Alt,
    bool IsVisible,
    bool IsMain,
    int SortOrder);
