using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.Delete.Specifications;
using GarmentPartEntity = Reference.Domain.GarmentPartOperations.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPart.Delete;

public sealed class DeleteGarmentPartCommandHandler(
    IReferenceRepository<GarmentPartEntity> repository,
    IReferenceReadRepository<GarmentPartOperationEntity> operationRepository)
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

        if (await operationRepository.AnyAsync(new GarmentPartOperationsOfPartSpec(command.Id), cancellationToken))
            return Result.Conflict("У групі є операції. Спершу перенесіть або видаліть їх.");

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
