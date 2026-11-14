using System.Data.Common;

namespace Reference.Contracts.Pricing;

/// <summary>Versioned pricing inputs of Reference (fabric, accessory and operation prices, other costs) valid at a moment.</summary>
public interface IPricingInputVersionReader
{
    /// <summary>
    /// Reads the versions valid at <paramref name="at"/>. When <paramref name="transaction"/> is given, the read joins it,
    /// so a caller's locked transaction sees the same data.
    /// </summary>
    Task<IReadOnlyList<PricingInputVersion>> ReadAsync(PricingInputRequest request, DateTimeOffset at,
        DbTransaction? transaction, CancellationToken cancellationToken);
}

public sealed record PricingInputRequest(
    IReadOnlyList<int> FabricIds,
    IReadOnlyList<int> AccessoryIds,
    IReadOnlyList<int> OperationIds,
    IReadOnlyList<string> OtherCostCodes);

/// <summary>
/// One input version; <paramref name="ReferenceKey"/> is <c>Fabric:{id}</c>, <c>Accessory:{id}</c>,
/// <c>Operation:{id}</c> or <c>OtherCost:{code}</c>.
/// </summary>
public sealed record PricingInputVersion(
    string ReferenceKey,
    long VersionId,
    decimal? Value,
    string Unit,
    bool IsActive,
    bool IsApproved,
    string SourceCode);
