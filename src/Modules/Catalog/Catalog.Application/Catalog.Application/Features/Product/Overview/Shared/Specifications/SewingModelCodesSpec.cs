using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Overview.Shared.Specifications;

/// <summary>All stored sewing model codes, for the next free MOD-XXX.</summary>
public sealed class SewingModelCodesSpec : Specification<ProductEntity, string>
{
    public SewingModelCodesSpec()
    {
        Query.Where(x => x.ModelCode != null).AsNoTracking();
        Query.Select(x => x.ModelCode!);
    }
}
