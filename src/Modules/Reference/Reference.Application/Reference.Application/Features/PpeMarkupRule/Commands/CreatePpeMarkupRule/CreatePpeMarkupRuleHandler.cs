using Reference.Application.Features.PpeMarkupRule.Shared;
using Reference.Domain.PpeMarkupRules;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.PpeMarkupRule;
using Reference.Application.Abstractions;
using BuildingBlocks.Application;

namespace Reference.Application.Features.PpeMarkupRule.Commands.CreatePpeMarkupRule;

public sealed class CreatePpeMarkupRuleHandler(
    IReferenceRepository<PpeMarkupRuleEntity> repository,
    IPricingSettingsChangeNotifier pricingSettingsChanged)
    : ICommandHandler<CreatePpeMarkupRuleCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreatePpeMarkupRuleCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();

        if (await repository.AnyAsync(new PpeMarkupRuleByNameSpec(name), cancellationToken))
            return Result.Invalid([new ValidationError("Request.Name", "Правило з такою назвою вже існує.")]);

        var existing = await repository.ListAsync(cancellationToken);
        var nextId = existing.Count == 0 ? 1 : existing.Max(x => x.Id.Value) + 1;

        var created = PpeMarkupRuleEntity.Create(PpeMarkupRuleId.From(nextId), name,
            PpeMarkupRuleDetailsFactory.From(request));
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;

        await repository.AddAsync(entity, cancellationToken);
        await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
