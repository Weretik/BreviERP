namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record ProductPhotoRequest(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
