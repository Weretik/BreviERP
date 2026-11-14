using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;
using Reference.Application.Abstractions;
using Reference.Application.Features.PricingParameter.Shared;

namespace Reference.Application.Features.PricingParameter.Commands.DeletePricingParameter;

public sealed class DeletePricingParameterHandler(
    IReferenceRepository<PricingParameterEntity> repository,
    IPricingSettingsChangeNotifier pricingSettingsChanged)
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
        await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);

        return Result.Success();
    }
}
