using Reference.Application.Features.PpeMarkupRule.Shared;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.PpeMarkupRule;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.PpeMarkupRule.Commands.UpdatePpeMarkupRule;

/// <summary>Edits markups and notes. The name stays: PPE supplier offers refer to the rule by it.</summary>
public sealed class UpdatePpeMarkupRuleHandler(
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

        var changed = entity.ApplyDetails(PpeMarkupRuleDetailsFactory.From(command.Request));
        if (changed.IsFailure) return changed.Error!.ToResult();

        await repository.UpdateAsync(entity, cancellationToken);
        await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);

        return Result.Success();
    }
}
