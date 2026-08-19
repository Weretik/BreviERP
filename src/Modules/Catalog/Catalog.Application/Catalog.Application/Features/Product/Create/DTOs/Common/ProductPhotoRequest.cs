namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record ProductPhotoRequest(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
