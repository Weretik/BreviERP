using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.Supplier.Commands.DeleteSupplier;

public sealed class DeleteSupplierHandler(IReferenceRepository<SupplierEntity> repository)
    : ICommandHandler<DeleteSupplierCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteSupplierCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new SupplierByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
