using Crm.Application.Customers.Access;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.Source;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Issues;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Documents.Reading;

public sealed class QuoteDocumentSource(CrmDbContext db) : IQuoteDocumentSource
{
    public async Task<QuoteIssue?> FindAuthorizedIssueAsync(Guid quotePublicId, Guid versionPublicId,
        CrmActor actor, CancellationToken cancellationToken)
    {
        if (quotePublicId == Guid.Empty || versionPublicId == Guid.Empty || actor.UserId <= 0) return null;
        var quote = await db.Quotes.AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == quotePublicId,
            cancellationToken);
        if (quote is null || (!actor.IsAdmin && quote.OwnerId.Value != actor.UserId)) return null;
        var version = await db.QuoteVersions.AsNoTracking().SingleOrDefaultAsync(
            x => x.QuoteId == quote.Id && x.PublicId == versionPublicId, cancellationToken);
        return version is null ? null : await db.Set<QuoteIssue>().AsNoTracking().SingleOrDefaultAsync(
            x => x.QuoteId == quote.Id && x.QuoteVersionId == version.Id, cancellationToken);
    }

    public Task<bool> CanReadQuoteAsync(Guid quotePublicId, QuoteId quoteId, CrmActor actor,
        CancellationToken cancellationToken)
    {
        if (quotePublicId == Guid.Empty || quoteId.Value <= 0 || actor.UserId <= 0)
            return Task.FromResult(false);
        return db.Quotes.AsNoTracking().AnyAsync(x => x.PublicId == quotePublicId && x.Id == quoteId &&
            (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)), cancellationToken);
    }

    public async Task<QuoteDocumentRenderSource?> LoadRenderSourceAsync(QuotePdfArtifactMetadata artifact,
        CancellationToken cancellationToken)
    {
        var issue = await db.Set<QuoteIssue>().AsNoTracking().SingleOrDefaultAsync(x =>
            x.QuoteId == artifact.QuoteId && x.QuoteVersionId == artifact.QuoteVersionId &&
            x.IssuerVersionId == artifact.IssuerVersionId &&
            x.TemplateVersionId == artifact.TemplateVersionId, cancellationToken);
        if (issue is null) return null;
        var version = await db.QuoteVersions.AsNoTracking().AsSplitQuery()
            .Include(x => x.Items).ThenInclude(x => x.Variants).ThenInclude(x => x.Tiers)
            .SingleOrDefaultAsync(x => x.Id == issue.QuoteVersionId && x.QuoteId == issue.QuoteId,
                cancellationToken);
        if (version is null) return null;
        var issuer = await db.IssuerSettingsVersions.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == issue.IssuerVersionId, cancellationToken);
        var template = await db.DocumentTemplateVersions.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == issue.TemplateVersionId, cancellationToken);
        if (issuer is null || template is null) return null;
        return new QuoteDocumentRenderSource(issue, version, issuer, template, issuer.LegalName);
    }
}
