using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.GetList.Specifications;
using Reference.Application.Features.GarmentPartOperation.GetList.DTOs;
using Reference.Application.Features.GarmentPartOperation.GetList.Specifications;
using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.GetList;

public sealed class GetGarmentPartOperationsQueryHandler(
    IReferenceReadRepository<GarmentPartOperationEntity> repository,
    IReferenceReadRepository<GarmentPartEntity> garmentPartRepository)
    : IQueryHandler<GetGarmentPartOperationsQuery, Result<List<GarmentPartOperationRowDTO>>>
{
    public async ValueTask<Result<List<GarmentPartOperationRowDTO>>> Handle(
        GetGarmentPartOperationsQuery query,
        CancellationToken cancellationToken)
    {
        var operations = await repository.ListAsync(new GetGarmentPartOperationsSpec(), cancellationToken);

        if (operations is { Count: 0 })
            return Result.NotFound();

        var garmentParts = await garmentPartRepository.ListAsync(new GetGarmentPartsSpec(), cancellationToken);
        var garmentPartNamesById = garmentParts.ToDictionary(x => x.Id, x => x.Name);

        var result = operations
            .Select(x => new GarmentPartOperationRowDTO(
                x.Id,
                garmentPartNamesById.GetValueOrDefault(x.GarmentPartId, string.Empty),
                x.Name,
                x.Min))
            .ToList();

        return Result.Success(result);
    }
}
