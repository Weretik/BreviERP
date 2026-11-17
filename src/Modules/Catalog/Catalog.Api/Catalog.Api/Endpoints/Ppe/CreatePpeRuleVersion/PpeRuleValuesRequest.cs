using System.Text.Json.Serialization;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Api.Endpoints.Ppe.CreatePpeRuleVersion;

public sealed record PpeRuleValuesRequest(
    [property: JsonRequired] string Code,
    [property: JsonRequired] PpeMarkupsRequest Markups,
    [property: JsonRequired] string Source,
    [property: JsonRequired] string SourceDate)
{
    /// <summary>Null when JSON sends <c>"markups": null</c>: the use-case validator answers with the Ukrainian message.</summary>
    public PpeRuleValues? ToValues() => Markups is null ? null :
        PpeRuleValues.Of(Code, Markups.Retail, Markups.Wholesale, Markups.LargeWholesale, Source, SourceDate);
}
