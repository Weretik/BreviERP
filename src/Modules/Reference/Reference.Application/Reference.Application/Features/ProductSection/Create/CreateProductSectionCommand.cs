using Reference.Application.Features.ProductSection.Create.DTOs;

namespace Reference.Application.Features.ProductSection.Create;

public sealed record CreateProductSectionCommand(CreateProductSectionCommandRequest Request) : ICommand<Result<int>>;
