using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;

namespace Reference.Domain.Pricing;

public sealed class ReferencePricingInputVersion
{
    public ReferencePricingInputVersionId Id { get; private set; }
    public FabricId? FabricId { get; private set; }
    public GarmentAccessoryId? AccessoryId { get; private set; }
    public GarmentPartOperationId? OperationId { get; private set; }
    public string? OtherCostCode { get; private set; }
    public decimal? Value { get; private set; }
    public PricingInputUnit Unit { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsApproved { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public string SourceCode { get; private set; } = null!;
    public string SourceHash { get; private set; } = null!;
    public string? SourceDate { get; private set; }

    private ReferencePricingInputVersion() { }

    public static ReferencePricingInputVersion Create(FabricId? fabricId, GarmentAccessoryId? accessoryId,
        GarmentPartOperationId? operationId, string? otherCostCode, decimal? value, PricingInputUnit unit,
        bool isActive, bool isApproved, int revision, DateTimeOffset effectiveFrom,
        string sourceCode, string sourceHash, string? sourceDate)
    {
        var references = (fabricId.HasValue ? 1 : 0) + (accessoryId.HasValue ? 1 : 0)
            + (operationId.HasValue ? 1 : 0) + (!string.IsNullOrWhiteSpace(otherCostCode) ? 1 : 0);
        if (references != 1 || revision <= 0 || sourceHash.Length != 64
            || string.IsNullOrWhiteSpace(sourceCode) || sourceCode.Length > 200)
            throw new ArgumentException("Exactly one reference and complete provenance are required.");
        if (!Enum.IsDefined(unit) || (fabricId.HasValue && unit != PricingInputUnit.UahPerMetre)
            || (accessoryId.HasValue && unit is not (PricingInputUnit.UahPerMetre or PricingInputUnit.UahPerPiece))
            || (operationId.HasValue && unit != PricingInputUnit.Minutes)
            || (otherCostCode is not null && unit != PricingInputUnit.UahPerService)
            || value is < 0 || (!operationId.HasValue && value is null)
            || (value.HasValue && (decimal.Round(value.Value, 8) != value || value > 999999999999.99999999m)))
            throw new ArgumentException("Pricing value/unit is incompatible with the reference.");
        return new ReferencePricingInputVersion
        {
            FabricId = fabricId,
            AccessoryId = accessoryId,
            OperationId = operationId,
            OtherCostCode = otherCostCode,
            Value = value,
            Unit = unit,
            IsActive = isActive,
            IsApproved = isApproved,
            Revision = revision,
            EffectiveFrom = effectiveFrom.ToUniversalTime(),
            SourceCode = sourceCode,
            SourceHash = sourceHash,
            SourceDate = string.IsNullOrWhiteSpace(sourceDate) ? null : sourceDate
        };
    }
}

