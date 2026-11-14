namespace Catalog.Api.Endpoints.Product.Write;

public sealed record FabricWriteRequest(int FabricId, bool IsPrimary, int SortOrder);
