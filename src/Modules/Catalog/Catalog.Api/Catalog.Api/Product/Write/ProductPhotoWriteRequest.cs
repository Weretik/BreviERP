namespace Catalog.Api.Product.Write;

public sealed record ProductPhotoWriteRequest(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
