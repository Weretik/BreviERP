using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPartOperation.Delete.Specifications;
using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Delete;

public sealed class DeleteGarmentPartOperationCommandHandler(IReferenceRepository<GarmentPartOperationEntity> repository)
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

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
