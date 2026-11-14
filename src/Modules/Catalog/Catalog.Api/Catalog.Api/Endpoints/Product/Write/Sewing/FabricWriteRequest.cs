namespace Catalog.Api.Endpoints.Product.Write.Sewing;

public sealed record FabricWriteRequest(int FabricId, bool IsPrimary, int SortOrder);
