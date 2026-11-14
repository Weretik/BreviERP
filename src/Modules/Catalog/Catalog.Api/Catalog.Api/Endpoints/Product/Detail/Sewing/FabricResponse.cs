namespace Catalog.Api.Endpoints.Product.Detail.Sewing;

public sealed record FabricResponse(int FabricId, string Name, decimal Price, bool IsPrimary, int SortOrder);
