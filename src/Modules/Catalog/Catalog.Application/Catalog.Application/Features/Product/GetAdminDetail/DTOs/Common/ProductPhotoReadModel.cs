namespace Catalog.Application.Features.Product.GetAdminDetail.DTOs;

public sealed record ProductPhotoReadModel(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
