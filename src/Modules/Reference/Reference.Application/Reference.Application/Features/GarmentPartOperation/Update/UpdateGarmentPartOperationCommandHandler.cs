using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPartOperation.Update.Specifications;
using GarmentPartEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Update;

public sealed class UpdateGarmentPartOperationCommandHandler(
    IReferenceRepository<GarmentPartOperationEntity> repository,
    IReferenceReadRepository<GarmentPartEntity> garmentPartRepository)
    : ICommandHandler<UpdateGarmentPartOperationCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateGarmentPartOperationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new GarmentPartOperationByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
        var name = request.Name.Trim();
        var garmentPartName = request.GarmentPartName.Trim();

        var garmentPart = await garmentPartRepository.FirstOrDefaultAsync(
            new GarmentPartByNameSpec(garmentPartName), cancellationToken);

        if (garmentPart is null)
            return Result.NotFound("Garment part was not found.");

        var duplicateExists = await repository.AnyAsync(
            new GarmentPartOperationByPartAndNameExceptIdSpec(command.Id, garmentPart.Id.Value, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Garment part operation with the same name for this garment part already exists.");

        entity.Update(garmentPart.Id, name, request.Min);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
