
namespace Reference.Application.Features.GarmentAccessory.Commands.CreateGarmentAccessory;

public sealed record CreateGarmentAccessoryCommand(CreateGarmentAccessoryCommandRequest Request) : ICommand<Result<int>>;
