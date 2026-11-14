
namespace Reference.Application.Features.ProductSection.Commands.CreateProductSection;

public sealed record CreateProductSectionCommand(CreateProductSectionCommandRequest Request) : ICommand<Result<int>>;
