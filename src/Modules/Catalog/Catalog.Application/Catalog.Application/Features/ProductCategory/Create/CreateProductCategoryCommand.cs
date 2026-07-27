using Catalog.Application.Features.ProductCategory.Create.DTOs;

namespace Catalog.Application.Features.ProductCategory.Create;

public sealed record CreateProductCategoryCommand(CreateProductCategoryCommandRequest Request) : ICommand<Result<int>>;


