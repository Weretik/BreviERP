
namespace Catalog.Application.Features.ProductCategory.Commands.UpdateProductCategory;

public sealed record UpdateProductCategoryCommand(int Id, UpdateProductCategoryCommandRequest Request) : ICommand<Result>;


