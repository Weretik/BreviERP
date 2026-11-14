
namespace Catalog.Application.Features.ProductCategory.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommand(CreateProductCategoryCommandRequest Request) : ICommand<Result<int>>;


