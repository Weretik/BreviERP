namespace Reference.Application.Features.Fabric.Commands.DeleteFabric;

public sealed record DeleteFabricCommand(int Id) : ICommand<Result>;
