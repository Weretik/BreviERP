using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.AdditionalReference.Create.Specifications;
using Reference.Domain.ValueObjects;
using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Create;

public sealed class CreateAdditionalReferenceCommandHandler(
    IReferenceRepository<AdditionalReferenceEntity> repository)
    : ICommandHandler<CreateAdditionalReferenceCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateAdditionalReferenceCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var id = AdditionalReferenceId.From(request.Id);

        var exists = await repository.AnyAsync(
            new AdditionalReferenceByIdKeyOrNameSpec(request.Id, request.Key, request.Name), cancellationToken);

        if (exists)
            return Result.Conflict("Additional reference with the same id, key, or name already exists.");

        var entity = AdditionalReferenceEntity.Create(
            id,
            request.Name,
            request.Key,
            request.Value,
            request.Unit,
            request.Description);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
