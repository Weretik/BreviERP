using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Supplier.Create.Specifications;
using Reference.Domain.ValueObjects;
using SupplierEntity = Reference.Domain.Entities.Supplier;

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

        var exists = await repository.AnyAsync(new SupplierByIdOrNameSpec(request.Id, name), cancellationToken);

        if (exists)
            return Result.Conflict("Supplier with the same id or name already exists.");

        var entity = SupplierEntity.Create(SupplierId.From(request.Id), name, request.Link);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
