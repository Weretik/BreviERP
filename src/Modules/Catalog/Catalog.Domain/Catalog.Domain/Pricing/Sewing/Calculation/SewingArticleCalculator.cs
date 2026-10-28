namespace Catalog.Domain.Pricing.Sewing;

public static class SewingArticleCalculator
{
    public static SewingArticleCalculation Calculate(SewingCalculationSettings settings,
        IReadOnlyList<SewingMaterialCostInput> materials, IReadOnlyList<SewingOperationCostInput> operations)
    {
        var blockers = SewingCalculationValidation.Validate(settings, materials, operations);
        if (blockers.Count != 0)
            return new SewingArticleCalculation(null, null, blockers);
        try
        {
            var minutes = operations.Sum(line => line.Quantity * (line.ManualMinutes ?? line.StandardMinutes)!.Value);
            var minuteCost = settings.MonthlySalary / (settings.WorkDays * settings.ShiftMinutes);
            var labor = minutes * minuteCost * (1m + settings.AssemblyRate);
            var breakdown = new SewingCostBreakdown(
                materials.Where(line => !line.IsOtherCost).Sum(line => line.Quantity * line.UnitCost!.Value),
                materials.Where(line => line.IsOtherCost).Sum(line => line.Quantity * line.UnitCost!.Value),
                minutes, minuteCost, labor,
                labor * settings.CuttingRate, labor * settings.MasterRate, labor * settings.ManagerRate,
                labor * settings.BonusRate,
                minutes * (1m + settings.AssemblyRate) * settings.MonthlyOverhead
                    / (settings.WorkDays * settings.ShiftMinutes * settings.SeamstressCount));
            var tiers = SewingTierPricing.Calculate(breakdown.FullCost, settings);
            return new SewingArticleCalculation(breakdown, tiers, []);
        }
        catch (OverflowException)
        {
            return new SewingArticleCalculation(null, null, [new("NumericOverflow", null, "Calculation exceeds decimal precision.")]);
        }
    }

}

