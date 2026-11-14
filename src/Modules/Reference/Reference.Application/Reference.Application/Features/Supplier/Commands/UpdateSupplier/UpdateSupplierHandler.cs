using Ardalis.Result;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Application.Abstractions;
using BuildingBlocks.Application;

namespace Reference.Application.Features.Supplier.Commands.UpdateSupplier;

public sealed class UpdateSupplierHandler(IReferenceRepository<SupplierEntity> repository)
    : ICommandHandler<UpdateSupplierCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new SupplierByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        if (entity.IsManagedByKeepinCrm)
        {
            return Result.Invalid([new ValidationError(
                "Request",
                "Картка цього постачальника оновлюється з KeepinCRM. У Brevi змінюються лише активність і нотатки.")]);
        }

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

        var duplicateExists = await repository.AnyAsync(
            new SupplierByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
        {
            return Result.Invalid([new ValidationError(
                "Request.Name",
                "Постачальник з такою назвою уже існує.")]);
        }

        var changed = entity.Update(name, link, contactPerson, phoneNumber, notes, email, request.PurchaseCategories);
        if (changed.IsFailure) return changed.Error!.ToResult();

        entity.SetActive(request.IsActive);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
