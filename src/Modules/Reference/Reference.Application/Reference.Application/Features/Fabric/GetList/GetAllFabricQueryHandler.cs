using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Fabric.GetList.DTOs;
using Reference.Application.Features.Fabric.GetList.Specifications;
using Reference.Application.Features.Supplier.GetList.Specifications;
using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;

namespace Reference.Application.Features.Fabric.GetList;

public sealed class GetAllFabricQueryHandler(
    IReferenceReadRepository<FabricEntity> referenceRepository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : IQueryHandler<GetAllFabricQuery, Result<List<FabricRowDTO>>>
{
    public async ValueTask<Result<List<FabricRowDTO>>> Handle(
        GetAllFabricQuery query,
        CancellationToken cancellationToken)
    {
        var fabrics = await referenceRepository.ListAsync(new AllFabricsSpec(), cancellationToken);

        if (fabrics is { Count: 0 })
            return Result.NotFound();

        var suppliers = await supplierRepository.ListAsync(new GetSuppliersSpec(), cancellationToken);
        var supplierNamesById = suppliers.ToDictionary(x => x.Id, x => x.Name);

        var result = fabrics
            .Select(f => new FabricRowDTO(
                f.Id,
                f.Name,
                f.Price,
                supplierNamesById.GetValueOrDefault(f.ProviderId, "�")))
            .ToList();

        return Result.Success(result);
    }
}
