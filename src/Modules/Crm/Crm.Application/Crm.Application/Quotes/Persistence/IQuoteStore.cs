using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Introductions;
using Crm.Domain.Quotes.Sending;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Persistence;

/// <summary>Persistence port of КП; implemented by the Crm DbContext.</summary>
public interface IQuoteStore
{
    DbSet<Quote> Quotes { get; }

    /// <summary>«Налаштування КП»: at most one row.</summary>
    DbSet<QuoteSettings> QuoteSettings { get; }

    /// <summary>Connected Gmail of each staff member (R-15).</summary>
    DbSet<StaffMailbox> StaffMailboxes { get; }

    /// <summary>Letters with a КП that Gmail accepted.</summary>
    DbSet<QuoteSending> QuoteSendings { get; }

    /// <summary>«Історія змін» of every КП (R-22).</summary>
    DbSet<QuoteEvent> QuoteEvents { get; }

    /// <summary>«Готові шаблони» of introductions (R-19).</summary>
    DbSet<QuoteIntroTemplate> QuoteIntroTemplates { get; }

    /// <summary>Saves; a stale <see cref="Quote.Version"/> throws <see cref="DbUpdateConcurrencyException"/>.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>True when the exception is a unique violation of the КП number.</summary>
    bool IsDuplicateNumber(DbUpdateException exception);

    /// <summary>True when two first saves of the settings raced (primary key violation).</summary>
    bool IsDuplicateSettings(DbUpdateException exception);

    /// <summary>True when two consents of the same staff member raced (primary key violation).</summary>
    bool IsDuplicateMailbox(DbUpdateException exception);

    /// <summary>True when a template with the same name (ignoring case) already exists.</summary>
    bool IsDuplicateTemplateName(DbUpdateException exception);
}
