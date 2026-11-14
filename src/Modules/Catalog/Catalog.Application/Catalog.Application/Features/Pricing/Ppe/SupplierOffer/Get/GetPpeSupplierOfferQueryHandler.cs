using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Shared;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Get;

public sealed class GetPpeSupplierOfferQueryHandler(
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
