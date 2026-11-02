using Reference.Application.Contracts.Catalog;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Shared.Dates;
using Reference.Application.Features.PricingParameter.Shared;
using Reference.Application.Features.PricingParameter.Shared.Specifications;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.PricingParameters.ValueObjects;
using PricingParameterEntity = Reference.Domain.PricingParameters.Entities.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Create;

/// <summary>Adds a parameter with the next free ID; «Дата оновлення» is today (Kyiv).</summary>
public sealed class CreatePricingParameterCommandHandler(
    IReferenceRepository<PricingParameterEntity> repository,
    TimeProvider timeProvider,
    IPricingSettingsChangeNotifier pricingSettingsChanged)
    : ICommandHandler<CreatePricingParameterCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreatePricingParameterCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var code = PricingParameterEntity.NormalizeCode(request.Code)!;

        if (await repository.AnyAsync(new PricingParameterByCodeSpec(code), cancellationToken))
            return Result.Invalid([new ValidationError("Request.Code", "Параметр з таким кодом уже існує.")]);

        var existing = await repository.ListAsync(cancellationToken);
        var nextId = existing.Count == 0 ? 1 : existing.Max(x => x.Id.Value) + 1;

        PricingParameterEntity entity;
        try
        {
            entity = PricingParameterEntity.Create(PricingParameterId.From(nextId), code,
                PricingParameterDetailsFactory.From(request), KyivDate.Today(timeProvider));
        }
        catch (DomainException exception)
        {
            return Result.Invalid([new ValidationError("Request", exception.Message)]);
        }

        await repository.AddAsync(entity, cancellationToken);
        await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
