using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.GetList.DTOs;
using Reference.Application.Features.GarmentPart.GetList.Specifications;
using Reference.Application.Features.Supplier.GetList.Specifications;
using SupplierEntity = Reference.Domain.Entities.Supplier;
using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.GetList;

public sealed class GetGarmentPartsQueryHandler(
    IReferenceReadRepository<GarmentPartEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : IQueryHandler<GetGarmentPartsQuery, Result<List<GarmentPartRowDTO>>>
{
    public async ValueTask<Result<List<GarmentPartRowDTO>>> Handle(
        GetGarmentPartsQuery query,
        CancellationToken cancellationToken)
    {
        var garmentParts = await repository.ListAsync(new GetGarmentPartsSpec(), cancellationToken);

        if (garmentParts is { Count: 0 })
            return Result.NotFound();

        var suppliers = await supplierRepository.ListAsync(new GetSuppliersSpec(), cancellationToken);
        var supplierNamesById = suppliers.ToDictionary(x => x.Id, x => x.Name);

        var result = garmentParts
            .Select(x => new GarmentPartRowDTO(
                x.Id.Value,
                x.Name,
                x.SupplierId.Value,
                supplierNamesById.GetValueOrDefault(x.SupplierId.Value, "-"),
                x.ContactPerson,
                x.PhoneNumber))
            .ToList();

        return Result.Success(result);
    }
}
