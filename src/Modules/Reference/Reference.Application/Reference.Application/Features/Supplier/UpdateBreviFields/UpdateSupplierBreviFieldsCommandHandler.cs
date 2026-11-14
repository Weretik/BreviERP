using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Supplier.Update.Specifications;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;

namespace Reference.Application.Features.Supplier.UpdateBreviFields;

public sealed class UpdateSupplierBreviFieldsCommandHandler(IReferenceRepository<SupplierEntity> repository)
    : ICommandHandler<UpdateSupplierBreviFieldsCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateSupplierBreviFieldsCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new SupplierByIdSpec(command.Id), cancellationToken);
        if (entity is null)
            return Result.NotFound();

        entity.SetActive(command.IsActive);
        entity.SetNotes(command.Notes);
        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
