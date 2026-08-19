using Catalog.Application.Contracts.Admin.Reference;

namespace Catalog.Application.Contracts.Admin.Ppe;

public sealed record PpePercentAdminDetail(
    PpePercentSource Source,
    AdditionalReferenceAdminDetail? Reference,
    decimal? CustomPercent);
