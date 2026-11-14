using ProductSectionEntity = Reference.Domain.ProductSections.ProductSection;
using Reference.Application.Abstractions;
using Reference.Application.Features.ProductSection.Shared;

namespace Reference.Application.Features.ProductSection.Commands.DeleteProductSection;

public sealed class DeleteProductSectionHandler(IReferenceRepository<ProductSectionEntity> repository)
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
