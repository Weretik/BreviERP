namespace Reference.Application.Features.GarmentPartOperation.Delete;

public sealed record DeleteGarmentPartOperationCommand(int Id) : ICommand<Result>;
