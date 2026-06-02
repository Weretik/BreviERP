namespace Reference.Application.Features.Fabric.GetList.DTOs;

public sealed record FabricRowDTO(int Id, string Name, decimal Price, string ProviderName);

internal sealed record FabricProjectionDTO(int Id, string Name, decimal Price, int ProviderId);
