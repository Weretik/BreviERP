using Reference.Application.Contracts.Catalog;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Shared.Dates;
using Reference.Application.Features.PricingParameter.Shared;
using Reference.Application.Features.PricingParameter.Shared.Specifications;
using BuildingBlocks.Domain.Exceptions;
using PricingParameterEntity = Reference.Domain.PricingParameters.Entities.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Update;

/// <summary>Edits a parameter card. The code stays; «Дата оновлення» becomes today when the value changes.</summary>
public sealed class UpdatePricingParameterCommandHandler(
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
        try
        {
            entity.ApplyDetails(details, valueChanged ? KyivDate.Today(timeProvider) : entity.UpdatedOn);
        }
        catch (DomainException exception)
        {
            return Result.Invalid([new ValidationError("Request", exception.Message)]);
        }

        await repository.UpdateAsync(entity, cancellationToken);
        await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);

        return Result.Success();
    }
}
