namespace Catalog.Api.Contracts.Products;

public sealed record FabricResponse(int FabricId, string Name, decimal Price, bool IsPrimary, int SortOrder);
