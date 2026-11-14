namespace Catalog.Api.Endpoints.Product.Write;

public sealed record ProductPhotoWriteRequest(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
