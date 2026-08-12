using Catalog.Application.Contracts.Reference;
using Mediator;
using Reference.Application.Features.AdditionalReference.GetList;
using Reference.Application.Features.Fabric.GetList;
using Reference.Application.Features.GarmentAccessory.GetList;
using Reference.Application.Features.GarmentPartOperation.GetList;
using Reference.Application.Features.Supplier.GetList;

namespace Host.Api.DependencyInjection.ServiceRegistration;

internal sealed class CatalogProductReferenceReader(ISender sender) : IProductReferenceReader
{
    public async Task<ProductReferenceData> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var suppliers = await sender.Send(new GetSuppliersQuery(), cancellationToken);
        var fabrics = await sender.Send(new GetAllFabricQuery(), cancellationToken);
        var accessories = await sender.Send(new GetGarmentAccessoriesQuery(), cancellationToken);
        var operations = await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken);
        var additionalReferences = await sender.Send(new GetAdditionalReferenceQuery(), cancellationToken);

        return new ProductReferenceData(
            suppliers.Value?.ToDictionary(x => x.Id, x => new ReferenceItem(x.Id, x.Name)) ?? [],
            fabrics.Value?.ToDictionary(x => x.Id, x => new PricedReferenceItem(x.Id, x.Name, x.Price)) ?? [],
            accessories.Value?.ToDictionary(x => x.Id, x => new PricedReferenceItem(x.Id, x.Name, x.Price)) ?? [],
            operations.Value?.ToDictionary(x => x.Id, x => new OperationReferenceItem(x.Id, x.Name, x.Min)) ?? [],
            additionalReferences.Value?.ToDictionary(x => x.Id, x => new AdditionalReferenceItem(x.Id, x.Name, x.Key, x.Value, x.Unit)) ?? []);
    }
}
