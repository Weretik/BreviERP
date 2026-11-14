namespace Catalog.Application.Features.Product.Shared.Detail.Sewing;

public sealed record FabricAdminDetail(int FabricId, string Name, decimal Price, bool IsPrimary, int SortOrder);
