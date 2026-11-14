
namespace Reference.Application.Features.Fabric.Commands.CreateFabric;

public sealed record CreateFabricCommand(CreateFabricCommandRequest Request) : ICommand<Result<int>>;
