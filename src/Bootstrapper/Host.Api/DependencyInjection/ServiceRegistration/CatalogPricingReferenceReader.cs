using Catalog.Application.Contracts.Pricing;
using Reference.Application.Contracts.Pricing;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Reference.Infrastructure.DataBase;

namespace Host.Api.DependencyInjection.ServiceRegistration;

internal sealed class CatalogPricingReferenceReader(
    IReferencePricingVersionReader reader, CatalogDbContext catalog, ReferenceDbContext reference) : IPricingReferenceVersionReader
{
    public async Task<IReadOnlyList<PricingReferenceValue>> ReadAsync(
        IReadOnlyCollection<string> referenceKeys, DateTimeOffset at, CancellationToken cancellationToken)
    {
        int[] Ids(string prefix) => referenceKeys.Where(x => x.StartsWith(prefix, StringComparison.Ordinal))
            .Select(x => int.Parse(x[prefix.Length..], System.Globalization.CultureInfo.InvariantCulture)).Distinct().ToArray();
        var request = new PricingReferenceRequest(Ids("Fabric:"), Ids("Accessory:"), Ids("Operation:"),
            referenceKeys.Where(x => x.StartsWith("OtherCost:", StringComparison.Ordinal)).Select(x => x[10..]).Distinct().ToArray());
        // Publication must read Reference inputs in the locked Catalog transaction.
        var transaction = catalog.Database.CurrentTransaction;
        if (transaction is not null)
            reference.Database.SetDbConnection(catalog.Database.GetDbConnection(), contextOwnsConnection: false);
        await using var enlisted = transaction is null ? null
            : await reference.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);
        var values = await reader.ReadAsync(request, at, cancellationToken);
        return values.Select(x => new PricingReferenceValue(
            x.ReferenceKey, x.VersionId, x.Value, x.Unit, x.IsActive, x.IsApproved, x.SourceCode)).ToArray();
    }
}

