using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPartOperation.Create.Specifications;
using Reference.Application.Features.GarmentPartOperation.Shared;
using Reference.Application.Features.GarmentPartOperation.Shared.Specifications;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using GarmentPartEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Create;

public sealed class CreateGarmentPartOperationCommandHandler(
    IReferenceRepository<GarmentPartOperationEntity> repository,
    IReferenceReadRepository<GarmentPartEntity> garmentPartRepository)
    : ICommandHandler<CreateGarmentPartOperationCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentPartOperationCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();
        var code = GarmentPartOperationEntity.NormalizeCode(request.Code);
        var garmentPart = await garmentPartRepository.FirstOrDefaultAsync(
            new GarmentPartByNameSpec(request.GarmentPartName.Trim()), cancellationToken);

        var errors = new List<ValidationError>();
        if (garmentPart is null)
            errors.Add(new ValidationError("Request.GarmentPartName", "Групу деталей з такою назвою не знайдено."));
        if (await repository.AnyAsync(new GarmentPartOperationByIdSpec(request.Id), cancellationToken))
            errors.Add(new ValidationError("Request.Id", "Операція з таким ідентифікатором уже існує."));
        if (garmentPart is not null && await repository.AnyAsync(
                new GarmentPartOperationByPartAndNameSpec(garmentPart.Id.Value, name), cancellationToken))
            errors.Add(new ValidationError("Request.Name", "У цій групі вже є операція з такою назвою."));
        if (code is not null && await repository.AnyAsync(new GarmentPartOperationByCodeSpec(code), cancellationToken))
            errors.Add(new ValidationError("Request.Code", "Операція з таким кодом уже існує."));
        if (errors.Count > 0)
            return Result.Invalid(errors);

        var created = GarmentPartOperationEntity.Create(
            GarmentPartOperationId.From(request.Id),
            GarmentPartOperationDetailsFactory.From(request, garmentPart!.Id));
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;

        await repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id.Value);
    }
}
