using Reference.Application.Features.GarmentAccessory.Create.DTOs;

namespace Reference.Application.Features.GarmentAccessory.Create;

public sealed record CreateGarmentAccessoryCommand(CreateGarmentAccessoryCommandRequest Request) : ICommand<Result<int>>;
