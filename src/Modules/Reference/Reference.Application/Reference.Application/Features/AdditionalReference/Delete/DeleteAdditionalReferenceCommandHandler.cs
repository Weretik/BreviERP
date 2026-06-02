using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.AdditionalReference.Delete.Specifications;
using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Delete;

public sealed class DeleteAdditionalReferenceCommandHandler(
    IReferenceRepository<AdditionalReferenceEntity> repository)
    : ICommandHandler<DeleteAdditionalReferenceCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteAdditionalReferenceCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new AdditionalReferenceByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
