namespace Catalog.Application.Features.Product.Photos.DTOs;

/// <summary>Photos of a product in display order (CAT-016); the first item is not always the main one.</summary>
public sealed record ProductPhotoList(int ProductId, IReadOnlyList<ProductPhotoItem> Photos);

/// <param name="Missing">True when the library no longer has the file; then the URLs are null.</param>
public sealed record ProductPhotoItem(
    int MediaFileId,
    string? FileName,
    string? Url,
    string? ThumbnailUrl,
    bool IsMain,
    int SortOrder,
    bool Missing);
