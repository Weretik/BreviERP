using Catalog.Application.Features.ProductCategory.Update.DTOs;

namespace Catalog.Application.Features.ProductCategory.Update;

public sealed record UpdateProductCategoryCommand(int Id, UpdateProductCategoryCommandRequest Request) : ICommand<Result>;


