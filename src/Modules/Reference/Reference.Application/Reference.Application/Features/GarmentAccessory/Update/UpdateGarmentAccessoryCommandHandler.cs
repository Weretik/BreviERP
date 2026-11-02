using Reference.Application.Contracts.Pricing;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Shared;
using Reference.Application.Features.GarmentAccessory.Shared.Specifications;
using Reference.Application.Features.GarmentAccessory.Update.Specifications;
using Reference.Application.Features.Shared.Dates;
using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Update;

public sealed class UpdateGarmentAccessoryCommandHandler(
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
