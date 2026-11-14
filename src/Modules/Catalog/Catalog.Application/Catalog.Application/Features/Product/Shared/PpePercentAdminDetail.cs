
namespace Catalog.Application.Features.Product.Shared;

public sealed record PpePercentAdminDetail(
    PpePercentSource Source,
    AdditionalReferenceAdminDetail? Reference,
    decimal? CustomPercent);
