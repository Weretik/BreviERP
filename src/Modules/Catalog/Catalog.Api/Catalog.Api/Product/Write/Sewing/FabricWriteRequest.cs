namespace Catalog.Api.Product.Write.Sewing;

public sealed record FabricWriteRequest(int FabricId, bool IsPrimary, int SortOrder);
