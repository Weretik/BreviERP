using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Contracts.Admin;
using Catalog.Application.Contracts.Admin.Product;

namespace Catalog.Application.Features.Product.Update;

public sealed record ReplaceProductCommand(int Id, CreateProductCommandRequest Request) : ICommand<Result<ProductAdminDetail>>;
