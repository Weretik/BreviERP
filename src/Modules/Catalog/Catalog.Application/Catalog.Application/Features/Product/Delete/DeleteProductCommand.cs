namespace Catalog.Application.Features.Product.Delete;

public sealed record DeleteProductCommand(int Id) : ICommand<Result>;
