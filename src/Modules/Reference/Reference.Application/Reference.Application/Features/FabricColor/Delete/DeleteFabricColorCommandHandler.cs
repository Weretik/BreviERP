using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.FabricColor.Shared;
using FabricColorEntity = Reference.Domain.FabricColors.Entities.FabricColor;

namespace Reference.Application.Features.FabricColor.Delete;

public sealed class DeleteFabricColorCommandHandler(IReferenceRepository<FabricColorEntity> repository)
    : ICommandHandler<DeleteFabricColorCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteFabricColorCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new FabricColorByIdSpec(command.Id), cancellationToken);
        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
