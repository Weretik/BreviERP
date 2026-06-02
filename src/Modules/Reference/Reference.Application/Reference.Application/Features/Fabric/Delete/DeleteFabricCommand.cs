namespace Reference.Application.Features.Fabric.Delete;

public sealed record DeleteFabricCommand(int Id) : ICommand<Result>;
