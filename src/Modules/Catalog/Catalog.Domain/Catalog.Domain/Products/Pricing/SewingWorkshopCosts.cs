namespace Catalog.Domain.Products.Pricing;

public sealed record SewingWorkshopCosts(
    decimal SeamstressMonthlySalary,
    decimal WorkDay,
    decimal SeamstressAwardPercent,
    decimal CutterCoefficientPercent,
    decimal MasterCoefficientPercent,
    decimal ForemanCoefficientPercent,
    decimal MonthlyExpenses,
    decimal SeamstressCount);
