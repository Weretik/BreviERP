using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.PpeMarkupRule;
using Reference.Application.Abstractions;
using Reference.Application.Features.PpeMarkupRule.Shared;

namespace Reference.Application.Features.PpeMarkupRule.Commands.DeletePpeMarkupRule;

public sealed class DeletePpeMarkupRuleHandler(
    IReferenceRepository<PpeMarkupRuleEntity> repository,
    IPricingSettingsChangeNotifier pricingSettingsChanged)
    : ICommandHandler<DeletePpeMarkupRuleCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeletePpeMarkupRuleCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new PpeMarkupRuleByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);
        await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);

        return Result.Success();
    }
}
