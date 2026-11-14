using System.Text.Json.Serialization;
namespace Catalog.Api.Pricing.Ppe;
public sealed record PpeMarkupsRequest(
    [property: JsonRequired] decimal Retail,
    [property: JsonRequired] decimal Wholesale,
    [property: JsonRequired] decimal LargeWholesale);
