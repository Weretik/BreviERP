namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record FabricRequest(int FabricId, bool IsPrimary, int SortOrder);
