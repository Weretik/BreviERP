using BuildingBlocks.Application.Helpers;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPart.Create.Specifications;
using Reference.Application.Features.GarmentPart.Update.Specifications;
using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;
using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.GarmentPart.Update;

public sealed class UpdateGarmentPartCommandHandler(
    IReferenceRepository<GarmentPartEntity> repository,
    IReferenceReadRepository<SupplierEntity> supplierRepository)
    : ICommandHandler<UpdateGarmentPartCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateGarmentPartCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new GarmentPartByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
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

        var duplicateExists = await repository.AnyAsync(
            new GarmentPartByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Garment part with the same name already exists.");

        entity.Update(name, supplierId, contactPerson, phoneNumber);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
