using Reference.Application.Features.GarmentPartOperation.Update.DTOs;

namespace Reference.Application.Features.GarmentPartOperation.Update;

public sealed record UpdateGarmentPartOperationCommand(int Id, UpdateGarmentPartOperationCommandRequest Request)
    : ICommand<Result>;
