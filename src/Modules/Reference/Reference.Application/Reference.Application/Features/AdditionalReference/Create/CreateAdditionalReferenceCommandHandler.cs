using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.AdditionalReference.Create.Specifications;
using Reference.Domain.ValueObjects;
using AdditionalReferenceEntity = Reference.Domain.Entities.AdditionalReference;

namespace Reference.Application.Features.AdditionalReference.Create;

public sealed class CreateAdditionalReferenceCommandHandler(
    IReferenceRepository<AdditionalReferenceEntity> repository)
    : ICommandHandler<CreateAdditionalReferenceCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateAdditionalReferenceCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var id = AdditionalReferenceId.From(request.Id);
        var name = request.Name.Trim();
        var key = request.Key.Trim();

        var idExists = await repository.AnyAsync(new AdditionalReferenceByIdSpec(request.Id), cancellationToken);
        var nameExists = await repository.AnyAsync(new AdditionalReferenceByNameSpec(name), cancellationToken);
        var keyExists = await repository.AnyAsync(new AdditionalReferenceByKeySpec(key), cancellationToken);

        if (idExists || nameExists || keyExists)
        {
            var validationErrors = new List<ValidationError>();

            if (idExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Id",
                    "Додатковий довідник з таким ідентифікатором уже існує."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "Додатковий довідник з такою назвою уже існує."));
            }

            if (keyExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Key",
                    "Додатковий довідник з таким ключем уже існує."));
            }

            return Result.Invalid(validationErrors);
        }

        var entity = AdditionalReferenceEntity.Create(
            id,
            name,
            key,
            request.Value,
            request.Unit,
            request.Description);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
