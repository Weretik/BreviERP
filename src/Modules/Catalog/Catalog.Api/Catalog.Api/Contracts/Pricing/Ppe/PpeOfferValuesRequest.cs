using System.Text.Json.Serialization;
using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Api.Contracts.Pricing.Ppe;
public sealed record PpeOfferValuesRequest(
    [property: JsonRequired] string? RuleCode,
    [property: JsonRequired] decimal? SupplierCost,
    [property: JsonRequired] decimal? ExternalRetail,
    [property: JsonRequired] decimal? ExternalWholesale,
    [property: JsonRequired] string Source,
    [property: JsonRequired] string SourceDate,
    [property: JsonRequired] bool SourceReviewed)
{
    public PpeOfferValues ToValues() => new(RuleCode, SupplierCost, ExternalRetail, ExternalWholesale, Source, SourceDate, SourceReviewed);
}
