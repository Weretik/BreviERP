using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Introductions;
using Crm.Domain.Quotes.Sending;
using Crm.Domain.Quotes.Settings;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Abstractions.Quotes;

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
    IQueryable<QuoteEvent> QuoteEvents { get; }

    /// <summary>«Готові шаблони» of introductions (R-19).</summary>
    DbSet<QuoteIntroTemplate> QuoteIntroTemplates { get; }

    /// <summary>Active Brevi customers by KeepinCRM client id, for the transfer of old КП (R-25).</summary>
    Task<IReadOnlyDictionary<string, QuoteCustomer>> FindKeepinCustomersAsync(IReadOnlyCollection<string> keepinIds,
        CancellationToken cancellationToken);

    /// <summary>An active, not merged customer chosen in «Кому» (CARD-001 R-02); null when there is none.</summary>
    Task<QuoteCustomer?> FindQuoteCustomerAsync(Guid publicId, CancellationToken cancellationToken);

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
