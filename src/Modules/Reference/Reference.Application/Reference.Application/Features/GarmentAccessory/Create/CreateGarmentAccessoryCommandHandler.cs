using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentAccessory.Create.Specifications;
using Reference.Application.Features.GarmentAccessory.Shared;
using Reference.Application.Features.GarmentAccessory.Shared.Specifications;
using Reference.Application.Features.Shared.Dates;
using Reference.Domain.GarmentAccessories.ValueObjects;
using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Create;

public sealed class CreateGarmentAccessoryCommandHandler(
    IReferenceRepository<GarmentAccessoryEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateGarmentAccessoryCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentAccessoryCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var supplier = await supplierRepository.FirstOrDefaultAsync(
            new SupplierByNameSpec(request.SupplierName.Trim()), cancellationToken);
        var code = GarmentAccessoryEntity.NormalizeCode(request.Code);

        var errors = new List<ValidationError>();
        if (supplier is null)
            errors.Add(new ValidationError("Request.SupplierName", "Постачальника з такою назвою не знайдено."));
        if (await repository.AnyAsync(new GarmentAccessoryByIdSpec(request.Id), cancellationToken))
            errors.Add(new ValidationError("Request.Id", "Фурнітура з таким ідентифікатором уже існує."));
        if (code is not null && await repository.AnyAsync(new GarmentAccessoryByCodeSpec(code), cancellationToken))
            errors.Add(new ValidationError("Request.Code", "Фурнітура з таким кодом уже існує."));
        if (errors.Count > 0)
            return Result.Invalid(errors);

        var created = GarmentAccessoryEntity.Create(
            GarmentAccessoryId.From(request.Id),
            GarmentAccessoryDetailsFactory.From(request, supplier!.Id),
            MoneyAmount.From(request.Price),
            KyivDate.Today(timeProvider));
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
