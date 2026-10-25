namespace Catalog.Domain.Pricing.Sewing;

public static class SewingCalculationValidation
{
    public static IReadOnlyList<SewingPricingBlocker> Validate(SewingCalculationSettings settings,
        IReadOnlyList<SewingMaterialCostInput> materials, IReadOnlyList<SewingOperationCostInput> operations)
    {
        var errors = new List<SewingPricingBlocker>(SewingSettingsValidation.Validate(settings));
        if (!materials.Any(line => line.IsFabric))
            errors.Add(new("MissingFabric", null, "No effective fabric is configured."));
        if (operations.Count == 0)
            errors.Add(new("MissingOperations", null, "No operations are configured."));
        foreach (var line in materials)
        {
            if (line.Quantity <= 0 || line.UnitCost is null or < 0)
                errors.Add(new("MissingMaterialCost", line.LineKey, "A positive quantity and a nonnegative unit cost are required."));
            if (!line.UnitMatches)
                errors.Add(new("UnitMismatch", line.LineKey, "Cost and composition units differ."));
            if (!line.IsActive || !line.IsApproved)
                errors.Add(new("UnapprovedReference", line.LineKey, "Material cost is inactive or not approved."));
            if (line.IsRequired && !line.IsReviewed)
                errors.Add(new("UnreviewedMaterial", line.LineKey, "Required material has not been reviewed."));
        }
        foreach (var line in operations)
        {
            if (line.Quantity <= 0 || (line.ManualMinutes ?? line.StandardMinutes) is null or <= 0)
                errors.Add(new("MissingOperationTime", line.LineKey, "Positive quantity and effective minutes are required."));
            if (!line.IsReviewed)
                errors.Add(new("UnreviewedOperation", line.LineKey, "Operation has not been reviewed."));
            if (!line.IsActive || !line.IsApproved)
                errors.Add(new("UnapprovedReference", line.LineKey, "Operation reference is inactive or not approved."));
        }
        return errors;
    }
}

