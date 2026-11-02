using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PricingParameter.Shared.Specifications;
using PricingParameterEntity = Reference.Domain.PricingParameters.Entities.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Delete;

public sealed class DeletePricingParameterCommandHandler(
    IReferenceRepository<PricingParameterEntity> repository)
    : ICommandHandler<DeletePricingParameterCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeletePricingParameterCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new PricingParameterByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
