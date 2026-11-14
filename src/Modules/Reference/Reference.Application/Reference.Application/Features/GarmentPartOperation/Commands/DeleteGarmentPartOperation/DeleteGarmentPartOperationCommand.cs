namespace Reference.Application.Features.GarmentPartOperation.Commands.DeleteGarmentPartOperation;

public sealed record DeleteGarmentPartOperationCommand(int Id) : ICommand<Result>;
