using System.Security.Cryptography;
using Ardalis.Result;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Pricing.Sewing;

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
                ? Result.Success(Selection(repeated)) : Result.Conflict("Цей запит уже виконано з іншими налаштуваннями. Оновіть сторінку.");
        var latest = await db.Set<SewingPricingSettingsVersion>().AsNoTracking()
            .OrderByDescending(x => x.Revision).FirstOrDefaultAsync(cancellationToken);
        var at = DateTimeOffset.UtcNow;
        // expectedVersionId 0 means "no version yet": the first version comes from «Параметри ціноутворення».
        var firstVersion = latest is null && expectedVersionId == 0;
        if (!firstVersion && (latest is null || latest.Id.Value != expectedVersionId || latest.EffectiveFrom >= at))
            return Result.Conflict("Налаштування вже змінили або є новіша версія. Оновіть сторінку.");
        var version = SewingPricingSettingsVersion.Create((latest?.Revision ?? 0) + 1, at, settings, hash, source, true);
        db.Add(version);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success(Selection(version));
    }

    private static SewingPricingSettingsSelection Selection(SewingPricingSettingsVersion version)
        => new(version.Id.Value, version.Revision, version.EffectiveFrom, version.FormulaVersion, version.ToSettings());
}
