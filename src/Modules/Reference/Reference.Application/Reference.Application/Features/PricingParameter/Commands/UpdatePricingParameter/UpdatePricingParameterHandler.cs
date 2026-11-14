using Reference.Application.Features.PricingParameter.Shared;
using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.PricingParameter.Commands.UpdatePricingParameter;

/// <summary>Edits a parameter card. The code stays; «Дата оновлення» becomes today when the value changes.</summary>
public sealed class UpdatePricingParameterHandler(
    IReferenceRepository<PricingParameterEntity> repository,
    TimeProvider timeProvider,
    IPricingSettingsChangeNotifier pricingSettingsChanged)
    : ICommandHandler<UpdatePricingParameterCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdatePricingParameterCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new PricingParameterByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var details = PricingParameterDetailsFactory.From(command.Request);
        var valueChanged = entity.Value != details.Value || entity.IsCalculated != details.IsCalculated;
        var changed = entity.ApplyDetails(details, valueChanged ? KyivDate.Today(timeProvider) : entity.UpdatedOn);
        if (changed.IsFailure) return changed.Error!.ToResult();

        await repository.UpdateAsync(entity, cancellationToken);
        await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);

        return Result.Success();
    }
}
