using System.Text.Json.Serialization;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Api.Endpoints.Ppe.CreatePpeOfferVersion;

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
