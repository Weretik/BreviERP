
namespace Reference.Application.Features.GarmentPart.Commands.UpdateGarmentPart;

public sealed record UpdateGarmentPartCommand(int Id, UpdateGarmentPartCommandRequest Request) : ICommand<Result>;
