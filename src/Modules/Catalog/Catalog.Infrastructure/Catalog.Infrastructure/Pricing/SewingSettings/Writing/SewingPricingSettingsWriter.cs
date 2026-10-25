using System.Security.Cryptography;
using Ardalis.Result;
using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Pricing.SewingSettings.Writing;

public sealed class SewingPricingSettingsWriter(CatalogDbContext db) : ISewingPricingSettingsWriter
{
    public async Task<Result<SewingPricingSettingsSelection>> AppendAsync(
        long expectedVersionId, Guid requestId, SewingCalculationSettings settings, string actor,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025004)", cancellationToken);
        var source = JsonSerializer.Serialize(new { requestId, settings, actor });
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"sewing-settings:{requestId:D}")));
        var repeated = await db.Set<SewingPricingSettingsVersion>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.SourceHash == hash, cancellationToken);
        if (repeated is not null)
            return repeated.ToSettings() == settings
                ? Result.Success(Selection(repeated)) : Result.Conflict("Request UUID was used with different settings.");
        var latest = await db.Set<SewingPricingSettingsVersion>().AsNoTracking()
            .OrderByDescending(x => x.Revision).FirstOrDefaultAsync(cancellationToken);
        var at = DateTimeOffset.UtcNow;
        if (latest is null || latest.Id.Value != expectedVersionId || latest.EffectiveFrom >= at)
            return Result.Conflict("Settings changed or a future settings version already exists.");
        var version = SewingPricingSettingsVersion.Create(latest.Revision + 1, at, settings, hash, source, true);
        db.Add(version);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success(Selection(version));
    }

    private static SewingPricingSettingsSelection Selection(SewingPricingSettingsVersion version)
        => new(version.Id.Value, version.Revision, version.EffectiveFrom, version.FormulaVersion, version.ToSettings());
}
