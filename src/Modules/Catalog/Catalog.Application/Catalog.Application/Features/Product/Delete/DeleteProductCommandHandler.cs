using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.Delete.Specifications;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Delete;

public sealed class DeleteProductCommandHandler(ICatalogRepository<ProductEntity> repository)
    : ICommandHandler<DeleteProductCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductByIdSpec(command.Id), cancellationToken);
        if (product is null) return Result.NotFound();
        await repository.DeleteAsync(product, cancellationToken);
        return Result.Success();
    }
}
