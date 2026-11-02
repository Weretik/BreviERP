using Reference.Application.Contracts.Catalog;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PpeMarkupRule.Shared;
using Reference.Application.Features.PpeMarkupRule.Shared.Specifications;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.PpeMarkupRules.ValueObjects;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.Entities.PpeMarkupRule;

namespace Reference.Application.Features.PpeMarkupRule.Create;

public sealed class CreatePpeMarkupRuleCommandHandler(
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

        PpeMarkupRuleEntity entity;
        try
        {
            entity = PpeMarkupRuleEntity.Create(PpeMarkupRuleId.From(nextId), name,
                PpeMarkupRuleDetailsFactory.From(request));
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
