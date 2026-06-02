using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.GetList.DTOs;
using Reference.Application.Features.GarmentPart.GetList.Specifications;
using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.GetList;

public sealed class GetGarmentPartsQueryHandler(IReferenceReadRepository<GarmentPartEntity> repository)
    : IQueryHandler<GetGarmentPartsQuery, Result<List<GarmentPartRowDTO>>>
{
    public async ValueTask<Result<List<GarmentPartRowDTO>>> Handle(
        GetGarmentPartsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.ListAsync(new GetGarmentPartsSpec(), cancellationToken);

        if (result is { Count: 0 })
            return Result.NotFound();

        return Result.Success(result);
    }
}
