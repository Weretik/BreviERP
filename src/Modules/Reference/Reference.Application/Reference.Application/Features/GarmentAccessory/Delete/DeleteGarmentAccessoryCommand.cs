namespace Reference.Application.Features.GarmentAccessory.Delete;

public sealed record DeleteGarmentAccessoryCommand(int Id) : ICommand<Result>;
