namespace Catalog.Api.Contracts.Products;

public sealed record ProductPhotoWriteRequest(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
