using Reference.Application.Features.GarmentPart.Create.DTOs;

namespace Reference.Application.Features.GarmentPart.Create;

public sealed record CreateGarmentPartCommand(CreateGarmentPartCommandRequest Request) : ICommand<Result<int>>;
