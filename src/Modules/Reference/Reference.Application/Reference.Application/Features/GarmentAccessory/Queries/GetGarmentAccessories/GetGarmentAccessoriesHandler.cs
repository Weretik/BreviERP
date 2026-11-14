using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;
using Reference.Application.Abstractions;
using Reference.Application.Features.Supplier.Queries.GetSuppliers;

namespace Reference.Application.Features.GarmentAccessory.Queries.GetGarmentAccessories;

public sealed class GetGarmentAccessoriesHandler(
    IReferenceReadRepository<GarmentAccessoryEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : IQueryHandler<GetGarmentAccessoriesQuery, Result<List<GarmentAccessoryRowDTO>>>
{
    public async ValueTask<Result<List<GarmentAccessoryRowDTO>>> Handle(
        GetGarmentAccessoriesQuery query,
        CancellationToken cancellationToken)
    {
        var accessories = await repository.ListAsync(new GetGarmentAccessoriesSpec(), cancellationToken);

        if (accessories is { Count: 0 })
            return Result.NotFound();

        var suppliers = await supplierRepository.ListAsync(new GetSuppliersSpec(), cancellationToken);
        var supplierNamesById = suppliers.ToDictionary(x => x.Id, x => x.Name);

        var result = accessories
            .Select(x => new GarmentAccessoryRowDTO(
                x.Id,
                x.Name,
                x.Price,
                supplierNamesById.GetValueOrDefault(x.SupplierId, "-"),
                x.Code,
                x.Unit,
                x.PriceUpdatedAt,
                x.IsActive,
                x.Comment))
            .ToList();

        return Result.Success(result);
    }
}
