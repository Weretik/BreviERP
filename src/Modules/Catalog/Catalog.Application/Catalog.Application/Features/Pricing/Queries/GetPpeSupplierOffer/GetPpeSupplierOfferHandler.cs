using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;

namespace Catalog.Application.Features.Pricing.Queries.GetPpeSupplierOffer;

public sealed class GetPpeSupplierOfferHandler(
    ICatalogReadRepository<ProductEntity> products,
    IProductReferenceReader references,
    IPpePricingInputReader inputs) : IQueryHandler<GetPpeSupplierOfferQuery, Result<PpeSupplierOffer>>
{
    public async ValueTask<Result<PpeSupplierOffer>> Handle(GetPpeSupplierOfferQuery query, CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(new PpeProductWithProfileSpec(query.ProductId), cancellationToken);
        if (product is null) return Result.NotFound();
        return Result.Success(await PpeSupplierOfferReader.ReadAsync(product, references, inputs, cancellationToken));
    }
}
