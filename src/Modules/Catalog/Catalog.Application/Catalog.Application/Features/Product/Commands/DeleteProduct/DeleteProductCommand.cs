namespace Catalog.Application.Features.Product.Commands.DeleteProduct;

public sealed record DeleteProductCommand(int Id) : ICommand<Result>;
