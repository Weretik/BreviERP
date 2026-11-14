using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.GarmentAccessory.Commands.DeleteGarmentAccessory;

public sealed class DeleteGarmentAccessoryHandler(IReferenceRepository<GarmentAccessoryEntity> repository, IProductUsageReader productUsageReader)
    : ICommandHandler<DeleteGarmentAccessoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteGarmentAccessoryCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new GarmentAccessoryByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        if (await productUsageReader.IsGarmentAccessoryUsedAsync(command.Id, cancellationToken))
            return Result.Conflict("Фурнітуру використовують товари, тому її не можна видалити.");

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
