using Domain.Reference.Entities;
using Reference.Application.Contracts.Persistence;

namespace Application.Reference.GetAdditionalReference;

public class GetAdditionalReferenceQueryHandler(IReferenceReadRepository<AdditionalReference> repository)
    : IQueryHandler<GetAdditionalReferenceQuery, Result<List<AdditionalReferenceRowDTO>>>
{
    public async ValueTask<Result<List<AdditionalReferenceRowDTO>>> Handle(
        GetAdditionalReferenceQuery query, CancellationToken cancellationToken)
    {
        var result = await repository.ListAsync(
            new GetAdditionalReferenceSpec(), cancellationToken);

        if (result is {Count: 0}) return Result.NotFound();

        return Result.Success(result);
    }
}
