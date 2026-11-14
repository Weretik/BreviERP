namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail;

public sealed record ProductPhotoReadModel(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
