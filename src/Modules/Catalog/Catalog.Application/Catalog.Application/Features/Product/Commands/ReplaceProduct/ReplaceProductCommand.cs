using Catalog.Application.Features.Product.Commands.CreateProduct;
using Catalog.Application.Features.Product.Shared;
using Catalog.Application.Features.Product.Shared.Detail;

namespace Catalog.Application.Features.Product.Commands.ReplaceProduct;

public sealed record ReplaceProductCommand(int Id, CreateProductCommandRequest Request) : ICommand<Result<ProductAdminDetail>>;
