using Crm.Application.Quotes.Persistence;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Settings.Reading;

internal static class QuoteSettingsQueries
{
    /// <summary>The saved settings, or null before the first save (then <see cref="QuoteSettingsDefaults"/> apply).</summary>
    public static Task<QuoteSettings?> FindSettingsAsync(this IQuoteStore store, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = tracking ? store.QuoteSettings : store.QuoteSettings.AsNoTracking();
        return query.SingleOrDefaultAsync(x => x.Id == QuoteSettings.SingletonId, cancellationToken);
    }
}
