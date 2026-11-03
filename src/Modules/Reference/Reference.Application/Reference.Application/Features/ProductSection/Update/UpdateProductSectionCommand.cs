using Reference.Application.Features.ProductSection.Update.DTOs;

namespace Reference.Application.Features.ProductSection.Update;

public sealed record UpdateProductSectionCommand(int Id, UpdateProductSectionCommandRequest Request) : ICommand<Result>;
