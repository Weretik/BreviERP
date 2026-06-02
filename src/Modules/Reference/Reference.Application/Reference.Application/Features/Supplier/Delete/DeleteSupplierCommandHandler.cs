using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Supplier.Delete.Specifications;
using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.Supplier.Delete;

public sealed class DeleteSupplierCommandHandler(IReferenceRepository<SupplierEntity> repository)
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
