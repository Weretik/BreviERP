using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Fabric.Create.Specifications;
using Reference.Domain.ValueObjects;
using SupplierEntity = Reference.Domain.Entities.Supplier;
using FabricEntity = Reference.Domain.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Create;

public sealed class CreateFabricCommandHandler(
    IReferenceRepository<FabricEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : ICommandHandler<CreateFabricCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateFabricCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();
        var providerName = request.ProviderName.Trim();

        var supplier = await supplierRepository.FirstOrDefaultAsync(new SupplierByNameSpec(providerName), cancellationToken);

        if (supplier is null)
            return Result.NotFound("Fabric provider was not found.");

        var exists = await repository.AnyAsync(new FabricByIdOrNameSpec(request.Id, name), cancellationToken);

        if (exists)
            return Result.Conflict("Fabric with the same id or name already exists.");

        var entity = FabricEntity.Create(
            FabricId.From(request.Id),
            name,
            MoneyAmount.From(request.Price),
            supplier.Id);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}


