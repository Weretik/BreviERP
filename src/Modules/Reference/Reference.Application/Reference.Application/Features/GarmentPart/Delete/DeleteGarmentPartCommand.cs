namespace Reference.Application.Features.GarmentPart.Delete;

public sealed record DeleteGarmentPartCommand(int Id) : ICommand<Result>;
