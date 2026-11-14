using System.Text.Json.Serialization;
using Catalog.Application.Abstractions.Pricing;
namespace Catalog.Api.Pricing.Ppe;
public sealed record PpeOfferValuesRequest(
    [property: JsonRequired] string? RuleCode,
    [property: JsonRequired] decimal? SupplierCost,
    [property: JsonRequired] decimal? ExternalRetail,
    [property: JsonRequired] decimal? ExternalWholesale,
    [property: JsonRequired] string Source,
    [property: JsonRequired] string SourceDate)
{
    public PpeOfferValues ToValues() => new(RuleCode, SupplierCost, ExternalRetail, ExternalWholesale, Source, SourceDate);
}
