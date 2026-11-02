using Reference.Application.Contracts.Catalog;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PpeMarkupRule.Shared;
using Reference.Application.Features.PpeMarkupRule.Shared.Specifications;
using BuildingBlocks.Domain.Exceptions;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.Entities.PpeMarkupRule;

namespace Reference.Application.Features.PpeMarkupRule.Update;

/// <summary>Edits markups and notes. The name stays: PPE supplier offers refer to the rule by it.</summary>
public sealed class UpdatePpeMarkupRuleCommandHandler(
    IReferenceRepository<PpeMarkupRuleEntity> repository,
    IPricingSettingsChangeNotifier pricingSettingsChanged)
    : ICommandHandler<UpdatePpeMarkupRuleCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdatePpeMarkupRuleCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new PpeMarkupRuleByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        try
        {
            entity.ApplyDetails(PpeMarkupRuleDetailsFactory.From(command.Request));
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
