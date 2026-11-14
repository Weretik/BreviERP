using Reference.Application.Features.GarmentAccessory.Shared;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;
using Reference.Application.Abstractions;
using Reference.Application.Abstractions.Pricing;
using BuildingBlocks.Domain;

namespace Reference.Application.Features.GarmentAccessory.Commands.UpdateGarmentAccessory;

public sealed class UpdateGarmentAccessoryHandler(
    IReferenceRepository<GarmentAccessoryEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository,
    TimeProvider timeProvider)
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
        var supplier = await supplierRepository.FirstOrDefaultAsync(
            new SupplierByNameSpec(request.SupplierName.Trim()), cancellationToken);

        if (supplier is null)
            return Result.Invalid([new ValidationError("Request.SupplierName", "Постачальника з такою назвою не знайдено.")]);

        var code = GarmentAccessoryEntity.NormalizeCode(request.Code);
        if (code is not null && await repository.AnyAsync(new GarmentAccessoryByCodeSpec(code, command.Id), cancellationToken))
            return Result.Invalid([new ValidationError("Request.Code", "Фурнітура з таким кодом уже існує.")]);

        entity.ApplyDetails(GarmentAccessoryDetailsFactory.From(request, supplier.Id));
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
