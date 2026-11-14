using FabricEntity = Reference.Domain.Fabrics.Fabric;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.Fabric.Commands.DeleteFabric;

public sealed class DeleteFabricHandler(IReferenceRepository<FabricEntity> repository, IProductUsageReader productUsageReader)
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
            return Result.Conflict("Тканину використовують товари, тому її не можна видалити.");

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
