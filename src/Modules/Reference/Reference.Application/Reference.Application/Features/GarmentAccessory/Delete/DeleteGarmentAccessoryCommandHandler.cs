using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Delete.Specifications;
using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Delete;

public sealed class DeleteGarmentAccessoryCommandHandler(IReferenceRepository<GarmentAccessoryEntity> repository)
    : ICommandHandler<DeleteGarmentAccessoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteGarmentAccessoryCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new GarmentAccessoryByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
