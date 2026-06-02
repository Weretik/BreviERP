namespace Reference.Api.Contracts.Fabrics;

public sealed record CreateFabricRequest(int Id, string Name, decimal Price, string ProviderName);
