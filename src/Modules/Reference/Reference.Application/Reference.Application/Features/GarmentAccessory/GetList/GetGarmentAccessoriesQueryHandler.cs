using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.GetList.DTOs;
using Reference.Application.Features.GarmentAccessory.GetList.Specifications;
using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.GetList;

public sealed class GetGarmentAccessoriesQueryHandler(
    IReferenceReadRepository<GarmentAccessoryEntity> repository)
    : IQueryHandler<GetGarmentAccessoriesQuery, Result<List<GarmentAccessoryRowDTO>>>
{
    public async ValueTask<Result<List<GarmentAccessoryRowDTO>>> Handle(
        GetGarmentAccessoriesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.ListAsync(new GetGarmentAccessoriesSpec(), cancellationToken);

        if (result is { Count: 0 })
            return Result.NotFound();

        return Result.Success(result);
    }
}
