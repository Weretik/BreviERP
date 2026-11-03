namespace Reference.Application.Features.ProductSection.Delete;

public sealed record DeleteProductSectionCommand(int Id) : ICommand<Result>;
