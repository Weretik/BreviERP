namespace Catalog.Domain.Pricing.Sewing;

public sealed record SewingCostBreakdown(
    decimal Materials, decimal OtherCosts, decimal TotalMinutes, decimal MinuteCost,
    decimal Seamstress, decimal Cutting, decimal Master, decimal Manager, decimal Bonus, decimal Overhead)
{
    public decimal MaterialsTotal => Materials + OtherCosts;
    public decimal FullCost => MaterialsTotal + Seamstress + Cutting + Master + Manager + Bonus + Overhead;
}

