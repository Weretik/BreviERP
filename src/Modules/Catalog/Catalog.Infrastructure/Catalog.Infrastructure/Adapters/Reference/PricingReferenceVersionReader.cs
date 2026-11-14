
using Microsoft.EntityFrameworkCore.Storage;

using Reference.Contracts.Pricing;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Adapters.Reference;

/// <summary>Versioned Reference pricing inputs for Sewing price calculation and publication.</summary>
internal sealed class PricingReferenceVersionReader(IPricingInputVersionReader reader, CatalogDbContext catalog)
    : IPricingReferenceVersionReader
{
    public async Task<IReadOnlyList<PricingReferenceValue>> ReadAsync(
        IReadOnlyCollection<string> referenceKeys, DateTimeOffset at, CancellationToken cancellationToken)
    {
        int[] Ids(string prefix) => referenceKeys.Where(x => x.StartsWith(prefix, StringComparison.Ordinal))
            .Select(x => int.Parse(x[prefix.Length..], System.Globalization.CultureInfo.InvariantCulture)).Distinct()
            .ToArray();
        var request = new PricingInputRequest(Ids("Fabric:"), Ids("Accessory:"), Ids("Operation:"),
            referenceKeys.Where(x => x.StartsWith("OtherCost:", StringComparison.Ordinal)).Select(x => x[10..])
                .Distinct().ToArray());
        // Publication must read Reference inputs in the locked Catalog transaction.
        var transaction = catalog.Database.CurrentTransaction?.GetDbTransaction();
        var values = await reader.ReadAsync(request, at, transaction, cancellationToken);
        return values.Select(x => new PricingReferenceValue(
            x.ReferenceKey, x.VersionId, x.Value, x.Unit, x.IsActive, x.IsApproved, x.SourceCode)).ToArray();
    }
}
