namespace Catalog.Api.Product.Detail.Media;

public sealed record ProductPhotoResponse(int MediaFileId, string Url, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
