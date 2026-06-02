using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.Delete.Specifications;
using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Delete;

public sealed class DeleteGarmentPartCommandHandler(IReferenceRepository<GarmentPartEntity> repository)
    : ICommandHandler<DeleteGarmentPartCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteGarmentPartCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new GarmentPartByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
