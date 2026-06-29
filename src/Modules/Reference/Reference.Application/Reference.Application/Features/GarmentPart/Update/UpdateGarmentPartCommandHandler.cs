using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.Update.Specifications;
using GarmentPartEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Update;

public sealed class UpdateGarmentPartCommandHandler(IReferenceRepository<GarmentPartEntity> repository)
    : ICommandHandler<UpdateGarmentPartCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateGarmentPartCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new GarmentPartByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var name = command.Request.Name.Trim();
        var duplicateExists = await repository.AnyAsync(
            new GarmentPartByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Garment part with the same name already exists.");

        entity.Update(name);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
