using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Fabric.Create.Specifications;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;

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
        {
            return Result.Invalid([new ValidationError(
                "Request.ProviderName",
                "������������� ������� � ����� ������ �� ��������.")]);
        }

        var idExists = await repository.AnyAsync(new FabricByIdSpec(request.Id), cancellationToken);
        var nameExists = await repository.AnyAsync(new FabricByNameSpec(name), cancellationToken);

        if (idExists || nameExists)
        {
            var validationErrors = new List<ValidationError>();

            if (idExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Id",
                    "������� � ����� �������������� ��� ����."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "������� � ����� ������ ��� ����."));
            }

            return Result.Invalid(validationErrors);
        }

        var entity = FabricEntity.Create(
            FabricId.From(request.Id),
            name,
            MoneyAmount.From(request.Price),
            supplier.Id);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
