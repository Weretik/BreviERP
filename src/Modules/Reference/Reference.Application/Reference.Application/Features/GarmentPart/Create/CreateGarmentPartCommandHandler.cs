using BuildingBlocks.Application.Helpers;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.Create.Specifications;
using Reference.Domain.ValueObjects;
using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;
using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.GarmentPart.Create;

public sealed class CreateGarmentPartCommandHandler(
    IReferenceRepository<GarmentPartEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : ICommandHandler<CreateGarmentPartCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentPartCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var id = GarmentPartId.From(request.Id);
        var name = request.Name.Trim();
        var supplierId = request.SupplierId;
        var contactPerson = string.IsNullOrWhiteSpace(request.ContactPerson) ? null : request.ContactPerson.Trim();

        var phoneNumber = (string?)null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber)
            && PhoneNumberHelper.TryParse(request.PhoneNumber, out var normalizedPhone))
        {
            phoneNumber = normalizedPhone;
        }

        var supplier = await supplierRepository.FirstOrDefaultAsync(new SupplierByIdSpec(supplierId), cancellationToken);

        if (supplier is null)
            return Result.NotFound("Garment part supplier was not found.");

        var exists = await repository.AnyAsync(
            new GarmentPartByIdOrNameSpec(request.Id, name), cancellationToken);

        if (exists)
            return Result.Conflict("Garment part with the same id or name already exists.");

        var entity = GarmentPartEntity.Create(id, name, supplierId, contactPerson, phoneNumber);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
