using Ardalis.Result;
using BuildingBlocks.Application.Helpers;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Supplier.Create.Specifications;
using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;

namespace Reference.Application.Features.Supplier.Create;

public sealed class CreateSupplierCommandHandler(IReferenceRepository<SupplierEntity> repository)
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

        var entity = SupplierEntity.Create(
            Reference.Domain.Suppliers.ValueObjects.SupplierId.From(request.Id),
            name,
            link,
            contactPerson,
            phoneNumber,
            notes);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
