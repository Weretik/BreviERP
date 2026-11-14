
namespace Reference.Application.Features.GarmentAccessory.Commands.UpdateGarmentAccessory;

public sealed record UpdateGarmentAccessoryCommand(int Id, UpdateGarmentAccessoryCommandRequest Request) : ICommand<Result>;
