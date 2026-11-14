using Reference.Application.Features.GarmentPartOperation.Shared;
using GarmentPartEntity = Reference.Domain.GarmentParts.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Application.Abstractions;
using Reference.Application.Abstractions.Pricing;
using Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation.Specs;

namespace Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation;

public sealed class UpdateGarmentPartOperationHandler(
    IReferenceRepository<GarmentPartOperationEntity> repository,
    IReferenceReadRepository<GarmentPartEntity> garmentPartRepository)
    : ICommandHandler<UpdateGarmentPartOperationCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateGarmentPartOperationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new GarmentPartOperationByIdSpec(command.Id), cancellationToken);
        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
        var name = request.Name.Trim();
        var code = GarmentPartOperationEntity.NormalizeCode(request.Code);
        var garmentPart = await garmentPartRepository.FirstOrDefaultAsync(
            new GarmentPartByNameSpec(request.GarmentPartName.Trim()), cancellationToken);
        if (garmentPart is null)
            return Result.Invalid([new ValidationError("Request.GarmentPartName", "Групу деталей з такою назвою не знайдено.")]);

        if (await repository.AnyAsync(
                new GarmentPartOperationByPartAndNameExceptIdSpec(command.Id, garmentPart.Id.Value, name), cancellationToken))
            return Result.Conflict("У цій групі вже є операція з такою назвою.");
        if (code is not null && await repository.AnyAsync(
                new GarmentPartOperationByCodeSpec(code, command.Id), cancellationToken))
            return Result.Invalid([new ValidationError("Request.Code", "Операція з таким кодом уже існує.")]);

        entity.ApplyDetails(GarmentPartOperationDetailsFactory.From(request, garmentPart.Id));
        try
        {
            await repository.UpdateAsync(entity, cancellationToken);
        }
        catch (ReferencePricingConflictException exception)
        {
            return Result.Conflict(exception.Message);
        }

        return Result.Success();
    }
}
