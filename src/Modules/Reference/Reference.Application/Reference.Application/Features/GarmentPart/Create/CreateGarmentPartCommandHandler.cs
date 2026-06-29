using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.Create.Specifications;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using GarmentPartEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Create;

public sealed class CreateGarmentPartCommandHandler(IReferenceRepository<GarmentPartEntity> repository)
    : ICommandHandler<CreateGarmentPartCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentPartCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var id = GarmentPartId.From(request.Id);
        var name = request.Name.Trim();

        var idExists = await repository.AnyAsync(new GarmentPartByIdSpec(request.Id), cancellationToken);
        var nameExists = await repository.AnyAsync(new GarmentPartByNameSpec(name), cancellationToken);

        if (idExists || nameExists)
        {
            var validationErrors = new List<ValidationError>();

            if (idExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Id",
                    "Частина виробу з таким ідентифікатором уже існує."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "Частина виробу з такою назвою уже існує."));
            }

            return Result.Invalid(validationErrors);
        }

        var entity = GarmentPartEntity.Create(id, name);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
