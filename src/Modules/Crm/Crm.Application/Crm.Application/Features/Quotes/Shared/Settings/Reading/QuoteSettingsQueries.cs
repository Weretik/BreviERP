using Crm.Domain.Quotes.Settings;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Shared.Settings.Reading;

internal static class QuoteSettingsQueries
{
    /// <summary>The saved settings, or null before the first save (then <see cref="QuoteSettingsDefaults"/> apply).</summary>
    public static Task<QuoteSettings?> FindSettingsAsync(this IQuoteStore store, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = tracking ? store.QuoteSettings : store.QuoteSettings.AsNoTracking();
        return query.SingleOrDefaultAsync(x => x.Id == QuoteSettings.SingletonId, cancellationToken);
    }

    /// <summary>What the КП documents use: the saved settings, else the defaults.</summary>
    public static async Task<QuoteSettingsContent> SettingsContentAsync(this IQuoteStore store,
        CancellationToken cancellationToken) =>
        (await store.FindSettingsAsync(tracking: false, cancellationToken))?.Content ?? QuoteSettingsDefaults.Content;

    /// <summary>Connected Gmail addresses by staff member.</summary>
    public static async Task<IReadOnlyDictionary<int, string>> MailboxEmailsAsync(this IQuoteStore store,
        CancellationToken cancellationToken) =>
        await store.StaffMailboxes.AsNoTracking().ToDictionaryAsync(x => x.UserId, x => x.Email, cancellationToken);
}
