using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductSection.Shared.Specifications;
using ProductSectionEntity = Reference.Domain.ProductSections.Entities.ProductSection;

namespace Reference.Application.Features.ProductSection.Delete;

public sealed class DeleteProductSectionCommandHandler(IReferenceRepository<ProductSectionEntity> repository)
    : ICommandHandler<DeleteProductSectionCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteProductSectionCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new ProductSectionByIdSpec(command.Id), cancellationToken);
        if (entity is null)
            return Result.NotFound();

        await repository.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
