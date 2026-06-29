using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.GetList.DTOs;
using Reference.Application.Features.GarmentAccessory.GetList.Specifications;
using Reference.Application.Features.Supplier.GetList.Specifications;
using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.GetList;

public sealed class GetGarmentAccessoriesQueryHandler(
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
                supplierNamesById.GetValueOrDefault(x.SupplierId, "-")))
            .ToList();

        return Result.Success(result);
    }
}
