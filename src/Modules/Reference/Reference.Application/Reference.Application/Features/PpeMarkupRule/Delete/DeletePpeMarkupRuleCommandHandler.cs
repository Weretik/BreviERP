using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PpeMarkupRule.Shared.Specifications;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.Entities.PpeMarkupRule;

namespace Reference.Application.Features.PpeMarkupRule.Delete;

public sealed class DeletePpeMarkupRuleCommandHandler(IReferenceRepository<PpeMarkupRuleEntity> repository)
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

        return Result.Success();
    }
}
