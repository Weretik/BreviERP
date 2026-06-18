using BuildingBlocks.Application.Helpers;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Supplier.Update.Specifications;
using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.Supplier.Update;

public sealed class UpdateSupplierCommandHandler(IReferenceRepository<SupplierEntity> repository)
    : ICommandHandler<UpdateSupplierCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new SupplierByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
        var name = request.Name.Trim();
        var link = string.IsNullOrWhiteSpace(request.Link) ? null : request.Link.Trim();
        var contactPerson = string.IsNullOrWhiteSpace(request.ContactPerson) ? null : request.ContactPerson.Trim();
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        var phoneNumber = (string?)null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber)
            && PhoneNumberHelper.TryParse(request.PhoneNumber, out var normalizedPhone))
        {
            phoneNumber = normalizedPhone;
        }

        var duplicateExists = await repository.AnyAsync(
            new SupplierByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Supplier with the same name already exists.");

        entity.Update(name, link, contactPerson, phoneNumber, notes);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
