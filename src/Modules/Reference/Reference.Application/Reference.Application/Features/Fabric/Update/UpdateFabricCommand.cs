using Reference.Application.Features.Fabric.Update.DTOs;

namespace Reference.Application.Features.Fabric.Update;

public sealed record UpdateFabricCommand(int Id, UpdateFabricCommandRequest Request) : ICommand<Result>;
