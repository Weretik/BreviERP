using System.Data.Common;

using Reference.Contracts.Pricing;
using Reference.Infrastructure.DataBase;
using Reference.Application.Abstractions;

namespace Reference.Infrastructure.Contracts.Pricing;

internal sealed class PricingInputVersionReader(IReferencePricingVersionReader reader, ReferenceDbContext reference)
    : IPricingInputVersionReader
{
    public async Task<IReadOnlyList<PricingInputVersion>> ReadAsync(PricingInputRequest request, DateTimeOffset at,
        DbTransaction? transaction, CancellationToken cancellationToken)
    {
        // A caller's locked transaction (Catalog price publication) must see the same Reference inputs.
        if (transaction?.Connection is { } connection)
            reference.Database.SetDbConnection(connection, contextOwnsConnection: false);
        await using var enlisted = transaction is null
            ? null
            : await reference.Database.UseTransactionAsync(transaction, cancellationToken);
        var values = await reader.ReadAsync(new PricingReferenceRequest(request.FabricIds, request.AccessoryIds,
            request.OperationIds, request.OtherCostCodes), at, cancellationToken);
        return values.Select(x => new PricingInputVersion(x.ReferenceKey, x.VersionId, x.Value, x.Unit, x.IsActive,
            x.IsApproved, x.SourceCode)).ToArray();
    }
}
