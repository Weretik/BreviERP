using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Create.Specifications;
using Reference.Application.Features.GarmentAccessory.Shared.Specifications;
using Reference.Domain.ValueObjects;
using SupplierEntity = Reference.Domain.Entities.Supplier;
using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Create;

public sealed class CreateGarmentAccessoryCommandHandler(
    IReferenceRepository<GarmentAccessoryEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : ICommandHandler<CreateGarmentAccessoryCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentAccessoryCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();
        var supplierName = request.SupplierName.Trim();

        var supplier = await supplierRepository.FirstOrDefaultAsync(
            new SupplierByNameSpec(supplierName), cancellationToken);

        if (supplier is null)
        {
            return Result.Invalid([new ValidationError(
                "Request.SupplierName",
                "Постачальника фурнітури з такою назвою не знайдено.")]);
        }

        var idExists = await repository.AnyAsync(new GarmentAccessoryByIdSpec(request.Id), cancellationToken);
        var nameExists = await repository.AnyAsync(new GarmentAccessoryByNameSpec(name), cancellationToken);

        if (idExists || nameExists)
        {
            var validationErrors = new List<ValidationError>();

            if (idExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Id",
                    "Фурнітура з таким ідентифікатором уже існує."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "Фурнітура з такою назвою уже існує."));
            }

            return Result.Invalid(validationErrors);
        }

        var entity = GarmentAccessoryEntity.Create(
            GarmentAccessoryId.From(request.Id),
            name,
            MoneyAmount.From(request.Price),
            supplier.Id.Value);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
