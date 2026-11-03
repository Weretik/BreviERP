using Catalog.Application.Contracts.Reference;
using Mediator;
using Reference.Application.Features.Fabric.GetList;
using Reference.Application.Features.GarmentAccessory.GetList;
using Reference.Application.Features.GarmentPartOperation.GetList;
using Reference.Application.Features.ProductSection.GetList;
using Reference.Application.Features.Supplier.GetList;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>Gives Catalog the sheet codes and names of Reference items through the Reference list queries.</summary>
internal sealed class CatalogProductReferenceCodeReader(ISender sender) : IProductReferenceCodeReader
{
    public async Task<ProductReferenceCodes> ReadAsync(CancellationToken cancellationToken)
    {
        var fabrics = await sender.Send(new GetAllFabricQuery(), cancellationToken);
        var accessories = await sender.Send(new GetGarmentAccessoriesQuery(), cancellationToken);
        var operations = await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken);
        var suppliers = await sender.Send(new GetSuppliersQuery(), cancellationToken);
        var sections = await sender.Send(new GetProductSectionsQuery(), cancellationToken);

        return new ProductReferenceCodes(
            Codes(fabrics.Value?.Select(x => (x.Id, x.Code))),
            Codes(accessories.Value?.Select(x => (x.Id, x.Code))),
            Codes(operations.Value?.Select(x => (x.Id, x.Code))),
            suppliers.Value?.Select(x => new ReferenceItem(x.Id, x.Name)).ToList() ?? [],
            sections.Value?.Select(x => new ProductSectionItem(x.Id, x.Type, x.Name, x.IsActive)).ToList() ?? []);
    }

    private static List<ReferenceCodeItem> Codes(IEnumerable<(int Id, string? Code)>? items)
        => items?.Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => new ReferenceCodeItem(x.Id, x.Code!)).ToList() ?? [];
}
