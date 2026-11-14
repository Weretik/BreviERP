
namespace Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation;

public sealed record UpdateGarmentPartOperationCommand(int Id, UpdateGarmentPartOperationCommandRequest Request)
    : ICommand<Result>;
