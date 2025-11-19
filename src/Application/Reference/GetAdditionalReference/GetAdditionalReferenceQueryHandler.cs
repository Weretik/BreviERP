using Application.Reference.Shared;
using Domain.Reference.Entities;

namespace Application.Reference.GetAdditionalReference;

public class GetAdditionalReferenceQueryHandler(IReferenceReadRepository<AdditionalReference> repository)
    : IQueryHandler<GetAdditionalReferenceQuery, Result<List<AdditionalReferenceRowDTO>>>
{
    public async ValueTask<Result<List<AdditionalReferenceRowDTO>>> Handle(
        GetAdditionalReferenceQuery query, CancellationToken cancellationToken)
    {
        var spec = new GetAdditionalReferenceSpec();
        var result = await repository.ListAsync(spec, cancellationToken);
        if (result is {Count: 0}) return Result.NotFound();

        return Result.Success(result);
    }
}
