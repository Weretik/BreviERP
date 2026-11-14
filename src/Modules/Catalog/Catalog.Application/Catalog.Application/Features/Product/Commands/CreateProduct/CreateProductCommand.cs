using Catalog.Application.Features.Product.Shared;
using Catalog.Application.Features.Product.Shared.Detail;

namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record CreateProductCommand(CreateProductCommandRequest Request) : ICommand<Result<ProductAdminDetail>>;
