using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Persistence;

/// <summary>Persistence port of КП; implemented by the Crm DbContext.</summary>
public interface IQuoteStore
{
    DbSet<Quote> Quotes { get; }

    /// <summary>«Налаштування КП»: at most one row.</summary>
    DbSet<QuoteSettings> QuoteSettings { get; }

    /// <summary>Saves; a stale <see cref="Quote.Version"/> throws <see cref="DbUpdateConcurrencyException"/>.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>True when the exception is a unique violation of the КП number.</summary>
    bool IsDuplicateNumber(DbUpdateException exception);

    /// <summary>True when two first saves of the settings raced (primary key violation).</summary>
    bool IsDuplicateSettings(DbUpdateException exception);
}
