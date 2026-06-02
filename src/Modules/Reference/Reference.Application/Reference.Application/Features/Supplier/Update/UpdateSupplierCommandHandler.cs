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

        var duplicateExists = await repository.AnyAsync(
            new SupplierByNameExceptIdSpec(command.Id, name), cancellationToken);

        if (duplicateExists)
            return Result.Conflict("Supplier with the same name already exists.");

        entity.Update(name, request.Link);

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
