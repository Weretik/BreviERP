using Reference.Application.Features.FabricColor.Shared;
using FabricColorEntity = Reference.Domain.FabricColors.FabricColor;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.FabricColor.Commands.DeleteFabricColor;

public sealed class DeleteFabricColorHandler(IReferenceRepository<FabricColorEntity> repository)
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
