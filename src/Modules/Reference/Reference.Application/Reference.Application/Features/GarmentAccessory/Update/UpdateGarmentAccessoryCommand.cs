using Reference.Application.Features.GarmentAccessory.Update.DTOs;

namespace Reference.Application.Features.GarmentAccessory.Update;

public sealed record UpdateGarmentAccessoryCommand(int Id, UpdateGarmentAccessoryCommandRequest Request) : ICommand<Result>;
