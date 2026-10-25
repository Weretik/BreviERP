using Catalog.Application.Contracts.Pricing;
using Reference.Application.Contracts.Pricing;

namespace SewingPricing.Jobs.Reading;

public sealed class JobPricingReferenceReader(IReferencePricingVersionReader reader) : IPricingReferenceVersionReader
{
    public async Task<IReadOnlyList<PricingReferenceValue>> ReadAsync(
        IReadOnlyCollection<string> referenceKeys, DateTimeOffset at, CancellationToken cancellationToken)
    {
        int[] Ids(string prefix) => referenceKeys.Where(x => x.StartsWith(prefix, StringComparison.Ordinal))
            .Select(x => int.Parse(x[prefix.Length..], System.Globalization.CultureInfo.InvariantCulture)).Distinct().ToArray();
        var request = new PricingReferenceRequest(Ids("Fabric:"), Ids("Accessory:"), Ids("Operation:"),
            referenceKeys.Where(x => x.StartsWith("OtherCost:", StringComparison.Ordinal)).Select(x => x[10..]).Distinct().ToArray());
        var values = await reader.ReadAsync(request, at, cancellationToken);
        return values.Select(x => new PricingReferenceValue(
            x.ReferenceKey, x.VersionId, x.Value, x.Unit, x.IsActive, x.IsApproved, x.SourceCode)).ToArray();
    }
}

