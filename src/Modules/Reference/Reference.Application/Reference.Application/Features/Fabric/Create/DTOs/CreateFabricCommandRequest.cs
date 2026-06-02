namespace Reference.Application.Features.Fabric.Create.DTOs;

public sealed record CreateFabricCommandRequest(int Id, string Name, decimal Price, string ProviderName);
