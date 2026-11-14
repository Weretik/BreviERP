namespace Catalog.Api.Endpoints.Product.Detail;

public sealed record ProductPhotoResponse(int MediaFileId, string Url, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
