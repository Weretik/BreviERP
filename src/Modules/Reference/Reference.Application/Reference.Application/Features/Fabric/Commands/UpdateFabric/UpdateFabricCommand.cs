
namespace Reference.Application.Features.Fabric.Commands.UpdateFabric;

public sealed record UpdateFabricCommand(int Id, UpdateFabricCommandRequest Request) : ICommand<Result>;
