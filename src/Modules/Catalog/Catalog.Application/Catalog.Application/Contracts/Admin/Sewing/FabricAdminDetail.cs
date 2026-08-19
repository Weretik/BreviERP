namespace Catalog.Application.Contracts.Admin.Sewing;

public sealed record FabricAdminDetail(int FabricId, string Name, decimal Price, bool IsPrimary, int SortOrder);
