using Reference.Application.Features.Fabric.Create.DTOs;

namespace Reference.Application.Features.Fabric.Create;

public sealed record CreateFabricCommand(CreateFabricCommandRequest Request) : ICommand<Result<int>>;
