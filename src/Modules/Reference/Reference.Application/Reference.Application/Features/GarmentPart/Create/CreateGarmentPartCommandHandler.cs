using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.Create.Specifications;
using Reference.Domain.ValueObjects;
using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Create;

public sealed class CreateGarmentPartCommandHandler(IReferenceRepository<GarmentPartEntity> repository)
    : ICommandHandler<CreateGarmentPartCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentPartCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var id = GarmentPartId.From(request.Id);
        var name = request.Name.Trim();

        var exists = await repository.AnyAsync(
            new GarmentPartByIdOrNameSpec(request.Id, name), cancellationToken);

        if (exists)
            return Result.Conflict("Garment part with the same id or name already exists.");

        var entity = GarmentPartEntity.Create(id, name);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
