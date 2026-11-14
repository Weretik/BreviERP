
namespace Catalog.Application.Features.Product.Shared.Detail.Ppe;

public sealed record PpePercentAdminDetail(
    PpePercentSource Source,
    AdditionalReferenceAdminDetail? Reference,
    decimal? CustomPercent);
