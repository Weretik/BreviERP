using Catalog.Application.Features.Product.Photos.DTOs;

namespace Catalog.Application.Features.Product.Photos.SkuIndex;

/// <summary>Every product with its sale SKUs, for imports that match old-site articles (CAT-016).</summary>
public sealed record GetProductSkuIndexQuery : IQuery<Result<IReadOnlyList<ProductSkuIndexItem>>>;
