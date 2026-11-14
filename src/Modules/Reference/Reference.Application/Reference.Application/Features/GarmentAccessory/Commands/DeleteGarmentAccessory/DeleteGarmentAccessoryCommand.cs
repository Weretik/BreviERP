namespace Reference.Application.Features.GarmentAccessory.Commands.DeleteGarmentAccessory;

public sealed record DeleteGarmentAccessoryCommand(int Id) : ICommand<Result>;
