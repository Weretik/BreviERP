using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Shared.Overview;

/// <summary>Product type as text in the overview API: "Sewing" or "Ppe".</summary>
public static class ProductOverviewTypes
{
    public static string Name(ProductType type) => type == ProductType.Ppe ? "Ppe" : "Sewing";

    public static bool TryParse(string? value, out ProductType type)
    {
        type = value switch { "Sewing" => ProductType.Sewing, "Ppe" => ProductType.Ppe, _ => 0 };
        return type != 0;
    }
}
