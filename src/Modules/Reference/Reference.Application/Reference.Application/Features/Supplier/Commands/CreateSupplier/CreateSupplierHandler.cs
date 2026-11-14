using Ardalis.Result;
using BuildingBlocks.Application.Helpers;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Application.Abstractions;
using Reference.Application.Features.Supplier.Commands.CreateSupplier.Specs;

namespace Reference.Application.Features.Supplier.Commands.CreateSupplier;

public sealed class CreateSupplierHandler(IReferenceRepository<SupplierEntity> repository)
    : ICommandHandler<CreateSupplierCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();
        var link = string.IsNullOrWhiteSpace(request.Link) ? null : request.Link.Trim();
        var contactPerson = string.IsNullOrWhiteSpace(request.ContactPerson) ? null : request.ContactPerson.Trim();
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        var phoneNumber = (string?)null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber)
            && PhoneNumberHelper.TryParse(request.PhoneNumber, out var normalizedPhone))
        {
            phoneNumber = normalizedPhone;
        }

        var idExists = await repository.AnyAsync(new SupplierByIdSpec(request.Id), cancellationToken);
        var nameExists = await repository.AnyAsync(new SupplierByNameSpec(name), cancellationToken);

        if (idExists || nameExists)
        {
            var validationErrors = new List<ValidationError>();

            if (idExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Id",
                    "Постачальник з таким ідентифікатором уже існує."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "Постачальник з такою назвою уже існує."));
            }

            return Result.Invalid(validationErrors);
        }

        var created = SupplierEntity.Create(
            Reference.Domain.Suppliers.SupplierId.From(request.Id),
            name,
            link,
            contactPerson,
            phoneNumber,
            notes);
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;
        var changed = entity.Update(name, link, contactPerson, phoneNumber, notes, email, request.PurchaseCategories);
        if (changed.IsFailure) return changed.Error!.ToResult<int>();
        entity.SetActive(request.IsActive);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
