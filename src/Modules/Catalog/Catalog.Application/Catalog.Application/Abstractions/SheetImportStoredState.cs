using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Abstractions;

/// <param name="CalculatedProductIds">Products that saved sewing calculations refer to.</param>
/// <param name="CalculatedArticleIds">Articles that saved sewing calculations refer to.</param>
public sealed record SheetImportStoredState(
    IReadOnlyList<ProductEntity> Products,
    IReadOnlySet<int> CalculatedProductIds,
    IReadOnlySet<long> CalculatedArticleIds);
