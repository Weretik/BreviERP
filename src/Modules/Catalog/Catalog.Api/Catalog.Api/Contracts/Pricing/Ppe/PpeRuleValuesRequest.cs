using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Api.Contracts.Pricing.Ppe;
public sealed record PpeRuleValuesRequest(
    [property: JsonRequired] string Code,
    [property: JsonRequired] [Required] PpeMarkupsRequest Markups,
    [property: JsonRequired] string Source,
    [property: JsonRequired] string SourceDate)
{
    public PpeRuleValues ToValues() => new(Code, Markups.ToMarkups(), Source, SourceDate);
}
