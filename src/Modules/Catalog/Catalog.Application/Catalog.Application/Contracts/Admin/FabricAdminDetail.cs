namespace Catalog.Application.Contracts.Admin;

public sealed record FabricAdminDetail(int FabricId, bool IsPrimary, int SortOrder);
