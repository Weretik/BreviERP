using Reference.Application.Contracts.Persistence;
using Reference.Application.Contracts.Catalog;
using Reference.Application.Features.Fabric.Delete.Specifications;
using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Delete;

public sealed class DeleteFabricCommandHandler(IReferenceRepository<FabricEntity> repository, IProductUsageReader productUsageReader)
    : ICommandHandler<DeleteFabricCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteFabricCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new FabricByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        if (await productUsageReader.IsFabricUsedAsync(command.Id, cancellationToken))
            return Result.Conflict("Fabric is used by one or more products.");

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
