using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;

namespace Crm.Application.Embroidery.Persistence;

/// <summary>Persistence port of «Ціна вишивки»; implemented by the Crm DbContext.</summary>
public interface IEmbroideryStore
{
    /// <summary>At most one row.</summary>
    DbSet<EmbroiderySettings> EmbroiderySettings { get; }

    /// <summary>«Збережені»: saved calculations, never changed after saving.</summary>
    DbSet<EmbroiderySavedCalculation> EmbroideryCalculations { get; }

    /// <summary>The display name of an active Crm customer, or null.</summary>
    Task<string?> FindCustomerNameAsync(Guid publicId, CancellationToken cancellationToken);

    /// <summary>Saves; a stale <see cref="EmbroiderySettings.Version"/> throws <see cref="DbUpdateConcurrencyException"/>.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>True when two first saves raced (primary key violation).</summary>
    bool IsDuplicateEmbroiderySettings(DbUpdateException exception);
}

internal static class EmbroideryStoreQueries
{
    public static Task<EmbroiderySettings?> FindSettingsAsync(this IEmbroideryStore store, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = tracking ? store.EmbroiderySettings : store.EmbroiderySettings.AsNoTracking();
        return query.SingleOrDefaultAsync(x => x.Id == Domain.Embroidery.Settings.EmbroiderySettings.SingletonId,
            cancellationToken);
    }
}
