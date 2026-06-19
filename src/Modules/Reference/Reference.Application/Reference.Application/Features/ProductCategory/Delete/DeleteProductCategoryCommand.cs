namespace Reference.Application.Features.ProductCategory.Delete;

public sealed record DeleteProductCategoryCommand(int Id) : ICommand<Result>;
