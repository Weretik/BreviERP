
namespace Reference.Application.Features.GarmentPartOperation.Commands.CreateGarmentPartOperation;

public sealed record CreateGarmentPartOperationCommand(CreateGarmentPartOperationCommandRequest Request)
    : ICommand<Result<int>>;
