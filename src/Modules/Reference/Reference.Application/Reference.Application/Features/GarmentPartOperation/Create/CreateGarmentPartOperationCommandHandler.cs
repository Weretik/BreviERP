using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPartOperation.Create.Specifications;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using GarmentPartEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Create;

public sealed class CreateGarmentPartOperationCommandHandler(
    IReferenceRepository<GarmentPartOperationEntity> repository,
    IReferenceReadRepository<GarmentPartEntity> garmentPartRepository)
    : ICommandHandler<CreateGarmentPartOperationCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateGarmentPartOperationCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();
        var garmentPartName = request.GarmentPartName.Trim();

        var garmentPart = await garmentPartRepository.FirstOrDefaultAsync(
            new GarmentPartByNameSpec(garmentPartName), cancellationToken);

        if (garmentPart is null)
        {
            return Result.Invalid([new ValidationError(
                "Request.GarmentPartName",
                "������� ������ � ����� ������ �� ��������.")]);
        }

        var idExists = await repository.AnyAsync(
            new GarmentPartOperationByIdSpec(request.Id), cancellationToken);
        var nameExists = await repository.AnyAsync(
            new GarmentPartOperationByPartAndNameSpec(garmentPart.Id.Value, name), cancellationToken);

        if (idExists || nameExists)
        {
            var validationErrors = new List<ValidationError>();

            if (idExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Id",
                    "�������� ������� ������ � ����� �������������� ��� ����."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "�������� � ����� ������ ��� ���� ��� ���� ������� ������."));
            }

            return Result.Invalid(validationErrors);
        }

        var entity = GarmentPartOperationEntity.Create(
            GarmentPartOperationId.From(request.Id),
            garmentPart.Id,
            name,
            request.Min);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
