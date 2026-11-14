using Reference.Application.Contracts.Catalog;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PricingParameter.Shared.Specifications;
using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Delete;

public sealed class DeletePricingParameterCommandHandler(
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
