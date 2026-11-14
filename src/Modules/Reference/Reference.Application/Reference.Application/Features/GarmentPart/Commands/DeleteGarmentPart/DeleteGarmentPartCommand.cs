namespace Reference.Application.Features.GarmentPart.Commands.DeleteGarmentPart;

public sealed record DeleteGarmentPartCommand(int Id) : ICommand<Result>;
