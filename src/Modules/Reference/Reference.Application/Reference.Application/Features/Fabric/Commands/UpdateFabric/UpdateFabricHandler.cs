using Reference.Application.Features.Fabric.Shared;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using FabricEntity = Reference.Domain.Fabrics.Fabric;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.Fabric.Commands.UpdateFabric;

public sealed class UpdateFabricHandler(
    IReferenceRepository<FabricEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository,
    TimeProvider timeProvider)
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
        var supplier = await supplierRepository.FirstOrDefaultAsync(
            new SupplierByNameSpec(request.ProviderName.Trim()), cancellationToken);

        if (supplier is null)
            return Result.Invalid([new ValidationError("Request.ProviderName", "Постачальника з такою назвою не знайдено.")]);

        var code = FabricEntity.NormalizeCode(request.Code);
        if (code is not null && await repository.AnyAsync(new FabricByCodeSpec(code, command.Id), cancellationToken))
            return Result.Invalid([new ValidationError("Request.Code", "Тканина з таким кодом уже існує.")]);

        entity.ApplyDetails(FabricDetailsFactory.From(request, supplier.Id));
        var price = MoneyAmount.From(request.Price);
        if (entity.Price != price)
            entity.SetPrice(price, KyivDate.Today(timeProvider));

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
