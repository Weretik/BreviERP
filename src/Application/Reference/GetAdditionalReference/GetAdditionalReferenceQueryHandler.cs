using Application.Reference.Shared;
using Domain.Reference.Entities;

namespace Application.Reference.GetAdditionalReference;

public class GetAdditionalReferenceQueryHandler(IReferenceReadRepository<AdditionalReference> referenceRepository)
    : IQueryHandler<GetAdditionalReferenceQuery, Result<AdditionalReferenceDTO>>
{
    public async ValueTask<Result<AdditionalReferenceDTO>> Handle(
        GetAdditionalReferenceQuery query, CancellationToken cancellationToken)
    {
        var referenceSpec = new GetAdditionalReferenceSpec();
        var result = await referenceRepository.FirstOrDefaultAsync(referenceSpec, cancellationToken);

        if (result is null) return Result.NotFound();

        return Result.Success(result);
    }
}
