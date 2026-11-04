namespace Catalog.Api.Contracts.Products.Photos;

/// <param name="MediaFileIds">Library photos of the product in display order.</param>
/// <param name="MainMediaFileId">The main photo; null keeps the current one or makes the first one main.</param>
public sealed record SetProductPhotosRequest(IReadOnlyList<int> MediaFileIds, int? MainMediaFileId);
