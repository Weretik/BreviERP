namespace Application.ExampleAggregate.Queries.GetCategories;

public sealed record GetCategoriesQuery  : IQuery<Result<List<CategoryDto>>> { }
