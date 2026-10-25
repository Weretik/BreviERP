using Reference.Application.Contracts.Pricing;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Pricing;
using Reference.Infrastructure.DataBase;

namespace Reference.Infrastructure.Pricing.Reading;

public sealed class ReferencePricingVersionReader(ReferenceDbContext db) : IReferencePricingVersionReader
{
    public async Task<IReadOnlyList<ReferencePricingVersionValue>> ReadAsync(
        PricingReferenceRequest request, DateTimeOffset at, CancellationToken cancellationToken)
    {
        at = at.ToUniversalTime();
        var fabricIds = request.FabricIds.ToArray();
        var accessoryIds = request.AccessoryIds.ToArray();
        var operationIds = request.OperationIds.ToArray();
        var codes = request.OtherCostCodes.ToArray();
        // Scalar PostgreSQL array parameters avoid the provider's nullable typed-ID array-converter failure.
        var candidates = db.Set<ReferencePricingInputVersion>().FromSqlInterpolated($"""
            SELECT * FROM reference."PricingInputVersions"
            WHERE "IsApproved" AND "EffectiveFrom" <= {at}
              AND ("FabricId" = ANY({fabricIds}) OR "AccessoryId" = ANY({accessoryIds})
                OR "OperationId" = ANY({operationIds}) OR "OtherCostCode" = ANY({codes}))
            """).AsNoTracking();
        var latest = candidates.Where(value => !candidates.Any(newer =>
            newer.FabricId == value.FabricId && newer.AccessoryId == value.AccessoryId
            && newer.OperationId == value.OperationId && newer.OtherCostCode == value.OtherCostCode
            && newer.EffectiveFrom > value.EffectiveFrom));
        return await latest.Select(value => new ReferencePricingVersionValue(
                value.FabricId.HasValue ? "Fabric:" + value.FabricId.Value.Value
                    : value.AccessoryId.HasValue ? "Accessory:" + value.AccessoryId.Value.Value
                    : value.OperationId.HasValue ? "Operation:" + value.OperationId.Value.Value
                    : "OtherCost:" + value.OtherCostCode,
                value.Id.Value, value.Value, UnitName(value.Unit), value.IsActive, value.IsApproved, value.SourceCode))
            .ToListAsync(cancellationToken);
    }

    private static string UnitName(PricingInputUnit unit) => unit switch
    {
        PricingInputUnit.UahPerMetre => "Metre",
        PricingInputUnit.UahPerPiece => "Piece",
        PricingInputUnit.UahPerService => "Service",
        PricingInputUnit.Minutes => "Minutes",
        _ => throw new InvalidOperationException("Unknown pricing input unit.")
    };
}

