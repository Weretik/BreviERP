using System.Text.Json.Serialization;
namespace Catalog.Api.Endpoints.Ppe.CreatePpeRuleVersion;

public sealed record PpeMarkupsRequest(
    [property: JsonRequired] decimal Retail,
    [property: JsonRequired] decimal Wholesale,
    [property: JsonRequired] decimal LargeWholesale);
