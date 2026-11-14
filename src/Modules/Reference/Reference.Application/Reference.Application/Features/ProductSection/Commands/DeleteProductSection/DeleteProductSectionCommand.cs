namespace Reference.Application.Features.ProductSection.Commands.DeleteProductSection;

public sealed record DeleteProductSectionCommand(int Id) : ICommand<Result>;
