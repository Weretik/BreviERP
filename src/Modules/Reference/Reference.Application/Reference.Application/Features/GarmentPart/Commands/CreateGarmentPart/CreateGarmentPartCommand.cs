
namespace Reference.Application.Features.GarmentPart.Commands.CreateGarmentPart;

public sealed record CreateGarmentPartCommand(CreateGarmentPartCommandRequest Request) : ICommand<Result<int>>;
