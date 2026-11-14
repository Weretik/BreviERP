using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using FabricEntity = Reference.Domain.Fabrics.Fabric;
using Reference.Application.Abstractions;
using Reference.Application.Features.Supplier.Queries.GetSuppliers;

namespace Reference.Application.Features.Fabric.Queries.GetAllFabric;

public sealed class GetAllFabricHandler(
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
                supplierNamesById.GetValueOrDefault(f.ProviderId, "-"),
                f.Code,
                f.NameRu,
                f.Composition,
                f.CompositionRu,
                f.Density,
                f.PriceUpdatedAt,
                f.IsActive,
                f.Comment,
                f.ProposalDescription,
                f.ProviderId))
            .ToList();

        return Result.Success(result);
    }
}
