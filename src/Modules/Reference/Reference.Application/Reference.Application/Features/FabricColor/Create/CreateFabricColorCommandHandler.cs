using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.FabricColor.Shared;
using Reference.Domain.FabricColors.ValueObjects;
using FabricColorEntity = Reference.Domain.FabricColors.Entities.FabricColor;

namespace Reference.Application.Features.FabricColor.Create;

public sealed class CreateFabricColorCommandHandler(IReferenceRepository<FabricColorEntity> repository)
    : ICommandHandler<CreateFabricColorCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CreateFabricColorCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.ListAsync(cancellationToken);
        if (FabricColorNames.Taken(existing, command.Request.Name))
            return Result.Invalid([new ValidationError("Request.Name", FabricColorNames.Duplicate)]);

        var nextId = existing.Count == 0 ? 1 : existing.Max(x => x.Id.Value) + 1;
        var created = FabricColorEntity.Create(FabricColorId.From(nextId), FabricColorNames.Details(command.Request));
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;

        await repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id.Value);
    }
}
