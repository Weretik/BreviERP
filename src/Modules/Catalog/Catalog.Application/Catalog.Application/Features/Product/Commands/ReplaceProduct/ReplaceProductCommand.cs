using Catalog.Application.Features.Product.Commands.CreateProduct;
using Catalog.Application.Features.Product.Shared;

namespace Catalog.Application.Features.Product.Commands.ReplaceProduct;

public sealed record ReplaceProductCommand(int Id, CreateProductCommandRequest Request) : ICommand<Result<ProductAdminDetail>>;
