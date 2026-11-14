using GarmentPartEntity = Reference.Domain.GarmentParts.GarmentPart;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.GarmentPart.Queries.GetGarmentParts;

public sealed class GetGarmentPartsHandler(IReferenceReadRepository<GarmentPartEntity> repository)
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
