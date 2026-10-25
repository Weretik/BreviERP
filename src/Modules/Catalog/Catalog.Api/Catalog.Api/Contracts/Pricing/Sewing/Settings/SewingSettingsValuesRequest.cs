using System.Text.Json.Serialization;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Api.Contracts.Pricing.Sewing.Settings;

public sealed record SewingSettingsValuesRequest(
    [property: JsonRequired] decimal MonthlySalary,
    [property: JsonRequired] decimal WorkDays,
    [property: JsonRequired] decimal ShiftMinutes,
    [property: JsonRequired] decimal SeamstressCount,
    [property: JsonRequired] decimal AssemblyRate,
    [property: JsonRequired] decimal CuttingRate,
    [property: JsonRequired] decimal MasterRate,
    [property: JsonRequired] decimal ManagerRate,
    [property: JsonRequired] decimal BonusRate,
    [property: JsonRequired] decimal MonthlyOverhead,
    [property: JsonRequired] decimal RetailMultiplier,
    [property: JsonRequired] decimal WholesaleMultiplier,
    [property: JsonRequired] decimal LargeWholesaleMultiplier)
{
    public SewingCalculationSettings ToSettings() => new(MonthlySalary, WorkDays, ShiftMinutes, SeamstressCount,
        AssemblyRate, CuttingRate, MasterRate, ManagerRate, BonusRate, MonthlyOverhead,
        RetailMultiplier, WholesaleMultiplier, LargeWholesaleMultiplier);
}
