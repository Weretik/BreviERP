using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.FabricColor.Shared;
using FabricColorEntity = Reference.Domain.FabricColors.FabricColor;

namespace Reference.Application.Features.FabricColor.Update;

/// <summary>Edits a colour. Items and stock lines keep the colour text they were saved with.</summary>
public sealed class UpdateFabricColorCommandHandler(IReferenceRepository<FabricColorEntity> repository)
    : ICommandHandler<UpdateFabricColorCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateFabricColorCommand command, CancellationToken cancellationToken)
    {
        var colors = await repository.ListAsync(cancellationToken);
        var entity = colors.FirstOrDefault(x => x.Id.Value == command.Id);
        if (entity is null)
            return Result.NotFound();
        if (FabricColorNames.Taken(colors, command.Request.Name, command.Id))
            return Result.Invalid([new ValidationError("Request.Name", FabricColorNames.Duplicate)]);

        var changed = entity.ApplyDetails(FabricColorNames.Details(command.Request));
        if (changed.IsFailure) return changed.Error!.ToResult();

        await repository.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
