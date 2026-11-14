namespace Catalog.Api.Endpoints.Product.Detail;

public sealed record FabricResponse(int FabricId, string Name, decimal Price, bool IsPrimary, int SortOrder);
