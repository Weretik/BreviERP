using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.AdditionalReference.GetList.DTOs;
using Reference.Application.Features.AdditionalReference.GetList.Specifications;
using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.GetList;

public sealed class GetAdditionalReferenceQueryHandler(IReferenceReadRepository<AdditionalReferenceEntity> repository)
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
