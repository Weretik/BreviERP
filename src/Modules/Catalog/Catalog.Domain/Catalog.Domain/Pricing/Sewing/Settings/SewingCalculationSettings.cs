namespace Catalog.Domain.Pricing.Sewing;

public sealed record SewingCalculationSettings(
    decimal MonthlySalary,
    decimal WorkDays,
    decimal ShiftMinutes,
    decimal SeamstressCount,
    decimal AssemblyRate,
    decimal CuttingRate,
    decimal MasterRate,
    decimal ManagerRate,
    decimal BonusRate,
    decimal MonthlyOverhead,
    decimal RetailMultiplier,
    decimal WholesaleMultiplier,
    decimal LargeWholesaleMultiplier);

