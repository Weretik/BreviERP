namespace Catalog.Domain.Products.Pricing;

public static class SewingPricing
{
    public static SewingPricingResult? Calculate(SewingPricingInput input)
    {
        var totalMinutes = input.OperationMinutes.Sum();
        var piecesPerShift = totalMinutes <= 0 ? (decimal?)null : 480m / (totalMinutes * 1.25m);
        var costs = input.WorkshopCosts;
        if (piecesPerShift is null || input.MetersPerProduct <= 0 || input.Fabrics.Count == 0 ||
            costs.WorkDay <= 0 || costs.CutterCoefficientPercent <= 0 || costs.MasterCoefficientPercent <= 0 ||
            costs.ForemanCoefficientPercent <= 0 || costs.SeamstressCount <= 0)
            return null;

        var seamstressSalary = (costs.SeamstressMonthlySalary / costs.WorkDay) / piecesPerShift.Value;
        var workshopSalary = seamstressSalary +
            seamstressSalary * costs.SeamstressAwardPercent / 100m +
            seamstressSalary / (costs.CutterCoefficientPercent / 100m) +
            seamstressSalary / (costs.MasterCoefficientPercent / 100m) +
            seamstressSalary / (costs.ForemanCoefficientPercent / 100m);
        var overhead = costs.MonthlyExpenses / costs.WorkDay / costs.SeamstressCount / piecesPerShift.Value;
        var margins = input.ProfitMargins;
        var byFabric = input.Fabrics.Select(fabric =>
        {
            var baseCost = input.MetersPerProduct * fabric.Price + input.AccessoriesCost + workshopSalary + overhead;
            return new SewingFabricPrice(
                fabric.FabricId,
                baseCost * (1m + margins.Price1To10Percent / 100m),
                baseCost * (1m + margins.Price11To39Percent / 100m),
                baseCost * (1m + margins.Price40PlusPercent / 100m));
        }).ToList();
        return new SewingPricingResult(piecesPerShift.Value, byFabric);
    }
}
