using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Pricing.Reading;

public sealed class SewingPricingSettingsReader(CatalogDbContext db) : ISewingPricingSettingsReader
{
    public async Task<IReadOnlyList<SewingPricingSettingsSelection>> ReadCandidatesAsync(
        DateTimeOffset at, CancellationToken cancellationToken)
    {
        at = at.ToUniversalTime();
        return await db.Set<SewingPricingSettingsVersion>().AsNoTracking()
            .Where(x => x.IsApproved && x.EffectiveFrom <= at)
            .OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Revision).Take(2)
            .Select(x => new SewingPricingSettingsSelection(x.Id.Value, x.Revision, x.EffectiveFrom, x.FormulaVersion,
                new SewingCalculationSettings(x.MonthlySalary, x.WorkDays, x.ShiftMinutes, x.SeamstressCount,
                    x.AssemblyRate, x.CuttingRate, x.MasterRate, x.ManagerRate, x.BonusRate, x.MonthlyOverhead,
                    x.RetailMultiplier, x.WholesaleMultiplier, x.LargeWholesaleMultiplier)))
            .ToListAsync(cancellationToken);
    }
}

