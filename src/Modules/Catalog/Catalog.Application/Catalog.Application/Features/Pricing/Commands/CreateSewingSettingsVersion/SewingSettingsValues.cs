using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Commands.CreateSewingSettingsVersion;

/// <summary>The thirteen sewing pricing settings as the page sends them; the handler turns them into the domain settings.</summary>
public sealed record SewingSettingsValues(
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
    decimal LargeWholesaleMultiplier)
{
    public SewingCalculationSettings ToSettings() => new(MonthlySalary, WorkDays, ShiftMinutes, SeamstressCount,
        AssemblyRate, CuttingRate, MasterRate, ManagerRate, BonusRate, MonthlyOverhead,
        RetailMultiplier, WholesaleMultiplier, LargeWholesaleMultiplier);

    public static SewingSettingsValues From(SewingCalculationSettings x) => new(x.MonthlySalary, x.WorkDays, x.ShiftMinutes,
        x.SeamstressCount, x.AssemblyRate, x.CuttingRate, x.MasterRate, x.ManagerRate, x.BonusRate, x.MonthlyOverhead,
        x.RetailMultiplier, x.WholesaleMultiplier, x.LargeWholesaleMultiplier);
}
