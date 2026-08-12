using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Contracts.Admin;

namespace Catalog.Application.Features.Product.Create;

public sealed record CreateProductCommand(CreateProductCommandRequest Request) : ICommand<Result<ProductAdminDetail>>;
