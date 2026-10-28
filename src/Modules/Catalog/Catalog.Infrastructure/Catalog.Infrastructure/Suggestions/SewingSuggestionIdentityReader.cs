using Catalog.Application.Contracts.Suggestions;
using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Suggestions;

public sealed class SewingSuggestionIdentityReader(CatalogDbContext db) : ISewingSuggestionIdentityReader
{
    private const int CandidateLimit = 2000;
    public IQueryable<SewingSuggestionIdentity> CandidatesQuery(string sourceSeason)
        => db.Products.AsNoTracking()
            .Where(x => x.Type == ProductType.Sewing && !x.IsDeleted && x.ModelCode != null
                && x.CatalogProfile != null && x.CatalogProfile.IsActive && x.CatalogProfile.IsSourceReviewed
                && x.CatalogProfile.Season == sourceSeason)
            .SelectMany(x => x.Articles.Where(a => a.IsActive), (p, a) => new { p.Id, ArticleKey = a.Id, p.ModelCode, p.Name })
            .OrderBy(x => x.Id).ThenBy(x => x.ArticleKey)
            .Select(x => new SewingSuggestionIdentity(x.Id.Value, x.ArticleKey.Value, x.ModelCode!, x.Name));

    public async Task<SewingSuggestionIdentityBatch> ReadAsync(string sourceSeason, CancellationToken ct)
    {
        var rows = await CandidatesQuery(sourceSeason).Take(CandidateLimit + 1).ToArrayAsync(ct);
        return rows.Length > CandidateLimit ? new([], true) : new(rows, false);
    }
}
