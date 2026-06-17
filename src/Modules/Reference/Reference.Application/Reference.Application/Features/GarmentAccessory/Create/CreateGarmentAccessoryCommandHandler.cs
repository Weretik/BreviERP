using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Create.Specifications;
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
            return Result.NotFound("Garment accessory supplier was not found.");

        var exists = await repository.AnyAsync(new GarmentAccessoryByIdOrNameSpec(request.Id, name), cancellationToken);

        if (exists)
            return Result.Conflict("Garment accessory with the same id or name already exists.");

        var entity = GarmentAccessoryEntity.Create(
            GarmentAccessoryId.From(request.Id),
            name,
            MoneyAmount.From(request.Price),
            supplier.Id.Value);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
