namespace Catalog.Application.Features.Product.Photos.DTOs;

/// <summary>
/// A product with the sale SKUs it sells under: the article SKUs of a sewing model, the site article of PPE.
/// Imports use it to find a product by an old-site article (CAT-016).
/// </summary>
public sealed record ProductSkuIndexItem(int ProductId, string Type, string Name, bool IsActive, IReadOnlyList<string> Skus);
