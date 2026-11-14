namespace Catalog.Application.Features.ProductCategory.Commands.DeleteProductCategory;

public sealed record DeleteProductCategoryCommand(int Id) : ICommand<Result>;


