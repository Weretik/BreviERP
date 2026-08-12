using Reference.Application.Contracts.Persistence;
using Reference.Application.Contracts.Catalog;
using Reference.Application.Features.GarmentPartOperation.Delete.Specifications;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Delete;

public sealed class DeleteGarmentPartOperationCommandHandler(IReferenceRepository<GarmentPartOperationEntity> repository, IProductUsageReader productUsageReader)
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
            return Result.Conflict("Garment part operation is used by one or more products.");

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
