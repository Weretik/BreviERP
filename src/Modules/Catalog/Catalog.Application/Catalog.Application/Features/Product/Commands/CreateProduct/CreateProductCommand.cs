using Catalog.Application.Features.Product.Shared;

namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record CreateProductCommand(CreateProductCommandRequest Request) : ICommand<Result<ProductAdminDetail>>;
