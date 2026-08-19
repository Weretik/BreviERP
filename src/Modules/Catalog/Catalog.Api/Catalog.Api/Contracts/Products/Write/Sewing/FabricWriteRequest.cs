namespace Catalog.Api.Contracts.Products;

public sealed record FabricWriteRequest(int FabricId, bool IsPrimary, int SortOrder);
