using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Shared.Specifications;
using Reference.Application.Features.GarmentAccessory.Update.Specifications;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Update;

public sealed class UpdateGarmentAccessoryCommandHandler(
    IReferenceRepository<GarmentAccessoryEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : ICommandHandler<UpdateGarmentAccessoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateGarmentAccessoryCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new GarmentAccessoryByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
        var name = request.Name.Trim();
        var supplierName = request.SupplierName.Trim();

        var supplier = await supplierRepository.FirstOrDefaultAsync(
            new SupplierByNameSpec(supplierName), cancellationToken);

        if (supplier is null)
            return Result.NotFound("Garment accessory supplier was not found.");

        var duplicateExists = await repository.AnyAsync(
            new GarmentAccessoryByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Garment accessory with the same name already exists.");

        entity.Update(name, MoneyAmount.From(request.Price), supplier.Id.Value);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
