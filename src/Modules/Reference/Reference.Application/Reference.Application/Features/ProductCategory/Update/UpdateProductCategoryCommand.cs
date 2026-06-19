using Reference.Application.Features.ProductCategory.Update.DTOs;

namespace Reference.Application.Features.ProductCategory.Update;

public sealed record UpdateProductCategoryCommand(int Id, UpdateProductCategoryCommandRequest Request) : ICommand<Result>;
