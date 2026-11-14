using System.Text.Json.Serialization;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Api.Pricing.Ppe;
public sealed record PpeMarkupsRequest(
    [property: JsonRequired] decimal Retail,
    [property: JsonRequired] decimal Wholesale,
    [property: JsonRequired] decimal LargeWholesale)
{
    public PpeMarkups ToMarkups() => new(Retail, Wholesale, LargeWholesale);
}
