using Reference.Application.Features.Fabric.Shared;
using Reference.Domain.GarmentAccessories;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using FabricEntity = Reference.Domain.Fabrics.Fabric;
using Reference.Application.Abstractions;
using Reference.Domain.Fabrics;

namespace Reference.Application.Features.Fabric.Commands.CreateFabric;

public sealed class CreateFabricHandler(
    IReferenceRepository<FabricEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateFabricCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateFabricCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var supplier = await supplierRepository.FirstOrDefaultAsync(
            new SupplierByNameSpec(request.ProviderName.Trim()), cancellationToken);
        var code = FabricEntity.NormalizeCode(request.Code);

        var errors = new List<ValidationError>();
        if (supplier is null)
            errors.Add(new ValidationError("Request.ProviderName", "Постачальника з такою назвою не знайдено."));
        if (await repository.AnyAsync(new FabricByIdSpec(request.Id), cancellationToken))
            errors.Add(new ValidationError("Request.Id", "Тканина з таким ідентифікатором уже існує."));
        if (code is not null && await repository.AnyAsync(new FabricByCodeSpec(code), cancellationToken))
            errors.Add(new ValidationError("Request.Code", "Тканина з таким кодом уже існує."));
        if (errors.Count > 0)
            return Result.Invalid(errors);

        var created = FabricEntity.Create(
            FabricId.From(request.Id),
            FabricDetailsFactory.From(request, supplier!.Id),
            MoneyAmount.From(request.Price),
            KyivDate.Today(timeProvider));
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
