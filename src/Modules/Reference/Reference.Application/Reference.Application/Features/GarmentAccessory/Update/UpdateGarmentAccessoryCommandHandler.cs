using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Update.Specifications;
using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Update;

public sealed class UpdateGarmentAccessoryCommandHandler(IReferenceRepository<GarmentAccessoryEntity> repository)
    : ICommandHandler<UpdateGarmentAccessoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateGarmentAccessoryCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new GarmentAccessoryByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
        var name = request.Name.Trim();

        var duplicateExists = await repository.AnyAsync(
            new GarmentAccessoryByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Garment accessory with the same name already exists.");

        entity.Update(name, MoneyAmount.From(request.Price));

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
