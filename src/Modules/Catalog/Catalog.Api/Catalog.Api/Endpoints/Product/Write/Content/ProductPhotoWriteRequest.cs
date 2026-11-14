namespace Catalog.Api.Endpoints.Product.Write.Content;

public sealed record ProductPhotoWriteRequest(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
