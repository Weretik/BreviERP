using Reference.Application.Features.PricingParameter.Shared;
using Reference.Domain.PricingParameters;
using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;
using Reference.Application.Abstractions;
using BuildingBlocks.Application;

namespace Reference.Application.Features.PricingParameter.Commands.CreatePricingParameter;

/// <summary>Adds a parameter with the next free ID; «Дата оновлення» is today (Kyiv).</summary>
public sealed class CreatePricingParameterHandler(
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

        var created = PricingParameterEntity.Create(PricingParameterId.From(nextId), code,
            PricingParameterDetailsFactory.From(request), KyivDate.Today(timeProvider));
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;

        await repository.AddAsync(entity, cancellationToken);
        await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
