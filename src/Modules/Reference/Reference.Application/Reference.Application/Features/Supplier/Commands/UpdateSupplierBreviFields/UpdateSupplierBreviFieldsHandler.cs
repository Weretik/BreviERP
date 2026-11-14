using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Application.Abstractions;
using Reference.Application.Features.Supplier.Commands.UpdateSupplier;

namespace Reference.Application.Features.Supplier.Commands.UpdateSupplierBreviFields;

public sealed class UpdateSupplierBreviFieldsHandler(IReferenceRepository<SupplierEntity> repository)
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
