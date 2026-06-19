using Reference.Application.Features.ProductCategory.Create.DTOs;

namespace Reference.Application.Features.ProductCategory.Create;

public sealed record CreateProductCategoryCommand(CreateProductCategoryCommandRequest Request) : ICommand<Result<int>>;
