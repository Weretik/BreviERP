using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.GarmentPartOperation.Commands.DeleteGarmentPartOperation;

public sealed class DeleteGarmentPartOperationHandler(IReferenceRepository<GarmentPartOperationEntity> repository, IProductUsageReader productUsageReader)
    : ICommandHandler<DeleteGarmentPartOperationCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteGarmentPartOperationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new GarmentPartOperationByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        if (await productUsageReader.IsGarmentPartOperationUsedAsync(command.Id, cancellationToken))
            return Result.Conflict("Операцію використовують товари, тому її не можна видалити.");

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
