namespace Catalog.Api.Contracts.Products;

public sealed record ProductPhotoResponse(int MediaFileId, string Url, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
