using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Fabric.Update.Specifications;
using SupplierEntity = Reference.Domain.Entities.Supplier;
using FabricEntity = Reference.Domain.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Update;

public sealed class UpdateFabricCommandHandler(
    IReferenceRepository<FabricEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : ICommandHandler<UpdateFabricCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateFabricCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new FabricByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
        var name = request.Name.Trim();
        var providerName = request.ProviderName.Trim();

        var supplier = await supplierRepository.FirstOrDefaultAsync(new SupplierByNameSpec(providerName), cancellationToken);

        if (supplier is null)
            return Result.NotFound("Fabric provider was not found.");

        var duplicateExists = await repository.AnyAsync(
            new FabricByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Fabric with the same name already exists.");

        entity.Update(name, MoneyAmount.From(request.Price), supplier.Id);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}


