using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Create.Specifications;
using Reference.Application.Features.GarmentAccessory.Shared.Specifications;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;

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
                "РџРѕСЃС‚Р°С‡Р°Р»СЊРЅРёРєР° С„СѓСЂРЅС–С‚СѓСЂРё Р· С‚Р°РєРѕСЋ РЅР°Р·РІРѕСЋ РЅРµ Р·РЅР°Р№РґРµРЅРѕ.")]);
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
                    "Р¤СѓСЂРЅС–С‚СѓСЂР° Р· С‚Р°РєРёРј С–РґРµРЅС‚РёС„С–РєР°С‚РѕСЂРѕРј СѓР¶Рµ С–СЃРЅСѓС”."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "Р¤СѓСЂРЅС–С‚СѓСЂР° Р· С‚Р°РєРѕСЋ РЅР°Р·РІРѕСЋ СѓР¶Рµ С–СЃРЅСѓС”."));
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
