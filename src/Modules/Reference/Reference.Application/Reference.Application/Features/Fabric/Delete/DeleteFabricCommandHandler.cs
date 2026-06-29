using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Fabric.Delete.Specifications;
using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Delete;

public sealed class DeleteFabricCommandHandler(IReferenceRepository<FabricEntity> repository)
    : ICommandHandler<DeleteFabricCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteFabricCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new FabricByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
