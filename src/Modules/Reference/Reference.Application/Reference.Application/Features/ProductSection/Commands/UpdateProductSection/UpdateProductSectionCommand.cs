
namespace Reference.Application.Features.ProductSection.Commands.UpdateProductSection;

public sealed record UpdateProductSectionCommand(int Id, UpdateProductSectionCommandRequest Request) : ICommand<Result>;
