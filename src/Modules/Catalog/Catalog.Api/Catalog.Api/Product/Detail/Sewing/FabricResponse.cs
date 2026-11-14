namespace Catalog.Api.Product.Detail.Sewing;

public sealed record FabricResponse(int FabricId, string Name, decimal Price, bool IsPrimary, int SortOrder);
