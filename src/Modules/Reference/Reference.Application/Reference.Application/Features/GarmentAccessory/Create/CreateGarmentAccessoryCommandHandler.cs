using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Create.Specifications;
using Reference.Domain.ValueObjects;
using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Create;

public sealed class CreateGarmentAccessoryCommandHandler(IReferenceRepository<GarmentAccessoryEntity> repository)
    : ICommandHandler<CreateGarmentAccessoryCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentAccessoryCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();

        var exists = await repository.AnyAsync(new GarmentAccessoryByIdOrNameSpec(request.Id, name), cancellationToken);

        if (exists)
            return Result.Conflict("Garment accessory with the same id or name already exists.");

        var entity = GarmentAccessoryEntity.Create(
            GarmentAccessoryId.From(request.Id),
            name,
            MoneyAmount.From(request.Price));

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
