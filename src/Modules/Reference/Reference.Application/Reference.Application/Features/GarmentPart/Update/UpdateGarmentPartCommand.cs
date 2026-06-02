using Reference.Application.Features.GarmentPart.Update.DTOs;

namespace Reference.Application.Features.GarmentPart.Update;

public sealed record UpdateGarmentPartCommand(int Id, UpdateGarmentPartCommandRequest Request) : ICommand<Result>;
