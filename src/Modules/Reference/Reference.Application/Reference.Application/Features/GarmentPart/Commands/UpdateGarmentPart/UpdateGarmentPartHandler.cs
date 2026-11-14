using GarmentPartEntity = Reference.Domain.GarmentParts.GarmentPart;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.GarmentPart.Commands.UpdateGarmentPart;

public sealed class UpdateGarmentPartHandler(IReferenceRepository<GarmentPartEntity> repository)
    : ICommandHandler<UpdateGarmentPartCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateGarmentPartCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new GarmentPartByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var name = command.Request.Name.Trim();
        var duplicateExists = await repository.AnyAsync(
            new GarmentPartByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Деталь виробу з такою назвою вже є.");

        entity.Update(name);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
