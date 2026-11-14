using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;

namespace Catalog.Application.Features.Product.Commands.DeleteProduct;

public sealed class DeleteProductHandler(ICatalogRepository<ProductEntity> repository)
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
