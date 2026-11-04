using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Persistence;

/// <summary>Persistence port of КП; implemented by the Crm DbContext.</summary>
public interface IQuoteStore
{
    DbSet<Quote> Quotes { get; }

    /// <summary>Saves; a stale <see cref="Quote.Version"/> throws <see cref="DbUpdateConcurrencyException"/>.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>True when the exception is a unique violation of the КП number.</summary>
    bool IsDuplicateNumber(DbUpdateException exception);
}
