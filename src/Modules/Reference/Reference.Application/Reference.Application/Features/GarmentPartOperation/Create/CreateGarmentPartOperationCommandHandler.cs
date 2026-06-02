using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPartOperation.Create.Specifications;
using Reference.Domain.ValueObjects;
using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Create;

public sealed class CreateGarmentPartOperationCommandHandler(
    IReferenceRepository<GarmentPartOperationEntity> repository,
    IReferenceReadRepository<GarmentPartEntity> garmentPartRepository)
    : ICommandHandler<CreateGarmentPartOperationCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentPartOperationCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();
        var garmentPartName = request.GarmentPartName.Trim();

        var garmentPart = await garmentPartRepository.FirstOrDefaultAsync(
            new GarmentPartByNameSpec(garmentPartName), cancellationToken);

        if (garmentPart is null)
            return Result.NotFound("Garment part was not found.");

        var exists = await repository.AnyAsync(
            new GarmentPartOperationByIdOrPartAndNameSpec(request.Id, garmentPart.Id.Value, name), cancellationToken);

        if (exists)
            return Result.Conflict("Garment part operation with the same id or name for this garment part already exists.");

        var entity = GarmentPartOperationEntity.Create(
            GarmentPartOperationId.From(request.Id),
            garmentPart.Id,
            name,
            request.Min);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
