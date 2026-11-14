using GarmentPartEntity = Reference.Domain.GarmentParts.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Application.Abstractions;
using Reference.Application.Features.GarmentPart.Queries.GetGarmentParts;

namespace Reference.Application.Features.GarmentPartOperation.Queries.GetGarmentPartOperations;

public sealed class GetGarmentPartOperationsHandler(
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
            .Select(x =>
            {
                var garmentPartName = garmentPartNamesById.TryGetValue(x.GarmentPartId, out var name)
                    ? name
                    : string.Empty;

                return new GarmentPartOperationRowDTO(x.Id, garmentPartName, x.Name, x.Min, x.Code, x.Element,
                    x.Variant, x.MinutesPerProduct, x.IsActive, x.Notes);
            })
            .ToList();

        return Result.Success(result);
    }
}
