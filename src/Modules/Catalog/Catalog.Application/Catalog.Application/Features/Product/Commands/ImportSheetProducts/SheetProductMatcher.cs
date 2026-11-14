using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>
/// Finds the stored product of a sheet row: by type and code (sewing model ID ignoring case, PPE site article), else
/// by the exact name in either name column among products of the same type.
/// </summary>
public sealed class SheetProductMatcher(IReadOnlyList<ProductEntity> products)
{
    private readonly Dictionary<(ProductType, string), ProductEntity> _byCode = products
        .Where(x => !string.IsNullOrWhiteSpace(x.Code))
        .GroupBy(x => (x.Type, Key(x.Type, x.Code!)))
        .ToDictionary(x => x.Key, x => x.OrderBy(p => p.Id.Value).First());

    public ProductEntity? Match(ProductType type, string code, string name)
    {
        if (_byCode.TryGetValue((type, Key(type, code)), out var byCode))
            return byCode;
        var trimmed = name.Trim();
        return products.Where(x => x.Type == type && (x.Name == trimmed || x.RuName == trimmed))
            .OrderBy(x => x.Id.Value).FirstOrDefault();
    }

    private static string Key(ProductType type, string code)
        => type == ProductType.Sewing ? code.Trim().ToUpperInvariant() : code.Trim();
}
