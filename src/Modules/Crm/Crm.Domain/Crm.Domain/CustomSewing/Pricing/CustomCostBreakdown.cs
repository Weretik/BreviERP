namespace Crm.Domain.CustomSewing.Pricing;

public sealed record CustomCostBreakdown(decimal Materials, decimal OtherCosts, decimal TotalMinutes,
    decimal MinuteCost, decimal Seamstress, decimal Cutting, decimal Master, decimal Manager,
    decimal Bonus, decimal Overhead, decimal Delivery, decimal Packaging, decimal ApplicationCost, decimal ApplicationSale);
