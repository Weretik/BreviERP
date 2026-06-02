using Reference.Application.Features.GarmentPartOperation.Create.DTOs;

namespace Reference.Application.Features.GarmentPartOperation.Create;

public sealed record CreateGarmentPartOperationCommand(CreateGarmentPartOperationCommandRequest Request)
    : ICommand<Result<int>>;
