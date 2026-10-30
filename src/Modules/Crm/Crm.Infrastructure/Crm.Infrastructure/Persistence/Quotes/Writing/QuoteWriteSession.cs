using Crm.Application.Quotes.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Versions;
using Crm.Domain.Quotes.Issues;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Quotes.Writing;

public sealed class QuoteWriteSession(CrmDbContext db) : IQuoteWriteSession
{
    public Task<Customer?> FindCustomerAsync(Guid publicId, CancellationToken cancellationToken)
        => db.Customers.Include(x => x.Contacts)
            .SingleOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);

    public Task<Quote?> FindQuoteAsync(Guid publicId, CancellationToken cancellationToken)
        => db.Quotes.SingleOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);

    public async Task<ActiveDocumentVersions?> FindActiveDocumentVersionsAsync(CancellationToken cancellationToken)
    {
        // Activation updates this singleton row, so a ready transaction pins one consistent pair.
        await db.Database.ExecuteSqlRawAsync(
            "SELECT \"Id\" FROM crm.\"DocumentSettingsState\" WHERE \"Id\" = 1 FOR SHARE", cancellationToken);
        var state = await db.DocumentSettingsStates.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        if (state?.ActiveIssuerVersionId is not { } issuerId ||
            state.ActiveTemplateVersionId is not { } templateId)
            return null;
        var issuer = await db.IssuerSettingsVersions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == issuerId, cancellationToken);
        var template = await db.DocumentTemplateVersions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        return issuer is null || template is null ? null : new ActiveDocumentVersions(issuer, template);
    }

    public Task<QuoteVersion?> FindVersionAsync(long id, CancellationToken cancellationToken)
        => db.QuoteVersions.AsSplitQuery()
            .Include(x => x.Items).ThenInclude(x => x.Variants).ThenInclude(x => x.Tiers)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(Quote quote) => db.Quotes.Add(quote);
    public Task<QuoteVersion?> FindVersionAsync(QuoteId quoteId, Guid publicId, CancellationToken cancellationToken)
        => db.QuoteVersions.AsSplitQuery()
            .Include(x => x.Items).ThenInclude(x => x.Variants).ThenInclude(x => x.Tiers)
            .SingleOrDefaultAsync(x => x.QuoteId == quoteId && x.PublicId == publicId, cancellationToken);
    public void Add(QuoteVersion version) => db.QuoteVersions.Add(version);
    public void Add(QuoteIssue issue) => db.QuoteIssues.Add(issue);
    public Task FlushAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
