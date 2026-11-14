using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Catalog.Application.Abstractions.Pricing;
namespace Catalog.Api.Endpoints.Ppe.CreatePpeRuleVersion;

public sealed record PpeRuleValuesRequest(
    [property: JsonRequired] string Code,
    [property: JsonRequired] [Required] PpeMarkupsRequest Markups,
    [property: JsonRequired] string Source,
    [property: JsonRequired] string SourceDate)
{
    public PpeRuleValues ToValues() =>
        PpeRuleValues.Of(Code, Markups.Retail, Markups.Wholesale, Markups.LargeWholesale, Source, SourceDate);
}
