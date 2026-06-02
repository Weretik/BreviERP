using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.AdditionalReference.Update.Specifications;
using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Update;

public sealed class UpdateAdditionalReferenceCommandHandler(
    IReferenceRepository<AdditionalReferenceEntity> repository)
    : ICommandHandler<UpdateAdditionalReferenceCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateAdditionalReferenceCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new AdditionalReferenceByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
        var duplicateExists = await repository.AnyAsync(
            new AdditionalReferenceByKeyOrNameExceptIdSpec(command.Id, request.Key, request.Name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Additional reference with the same key or name already exists.");

        entity.Update(
            request.Name,
            request.Key,
            request.Value,
            request.Unit,
            request.Description);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
