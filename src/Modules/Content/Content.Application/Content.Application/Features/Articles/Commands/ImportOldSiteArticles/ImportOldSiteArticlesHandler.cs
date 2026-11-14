using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;

namespace Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

public sealed class ImportOldSiteArticlesHandler(
    IContentDbContext db, IContentPhotoImporter photos, TimeProvider clock)
    : ICommandHandler<ImportOldSiteArticlesCommand, Result<OldSiteArticleImportResponse>>
{
    public const string NoTitle = "У статті немає назви українською.";
    public const string Duplicate = "Стаття з такою адресою вже є вище у файлі.";
    public const string PhotoFailed = "Фото не перенесено";

    public async ValueTask<Result<OldSiteArticleImportResponse>> Handle(ImportOldSiteArticlesCommand command,
        CancellationToken cancellationToken)
    {
        var existing = await db.Articles.Include(x => x.Texts).ToDictionaryAsync(x => x.Slug, StringComparer.Ordinal,
            cancellationToken);
        var problems = new List<OldSiteImportNote>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var toWrite = new List<OldSiteArticle>();
        int created = 0, updated = 0, kept = 0;

        foreach (var item in command.Items)
        {
            if (!seen.Add(item.Slug)) problems.Add(Note(item, Duplicate));
            else if (ProductPageRules.Clean(item.Uk?.Title) is null) problems.Add(Note(item, NoTitle));
            else if (!existing.ContainsKey(item.Slug))
            {
                created++;
                toWrite.Add(item);
            }
            else if (command.Overwrite)
            {
                updated++;
                toWrite.Add(item);
            }
            else kept++;
        }

        var urls = toWrite.SelectMany(PhotoUrls).Select(OldSiteArticlePhotos.Parse).Where(x => x is not null)
            .Select(x => x!).DistinctBy(OldSiteArticlePhotos.SourceKey).ToList();
        var imported = new Dictionary<string, int>(
            await photos.FindImportedAsync(urls.Select(OldSiteArticlePhotos.SourceKey).ToArray(), cancellationToken),
            StringComparer.Ordinal);
        var toDownload = urls.Count(url => !imported.ContainsKey(OldSiteArticlePhotos.SourceKey(url)));
        if (command.DryRun)
            return new OldSiteArticleImportResponse(true, command.Items.Count, created, updated, kept, toDownload, 0, problems);

        var downloaded = 0;
        var failed = new Dictionary<string, string>(StringComparer.Ordinal);
        var now = clock.GetUtcNow();
        foreach (var item in toWrite)
        {
            var lost = new List<string>();
            var cover = item.CoverUrl is null ? null : await PhotoAsync(item.CoverUrl);
            var texts = new Dictionary<string, ArticleTextInput>();
            if (item.Uk is not null) texts[ProductPageRules.Ukrainian] = await TextAsync(item.Uk);
            if (item.Ru is not null) texts[ProductPageRules.Russian] = await TextAsync(item.Ru);

            if (!existing.TryGetValue(item.Slug, out var article))
            {
                var made = Article.Create(item.Slug, item.PublishedOn, command.UpdatedBy, now);
                if (made.IsFailure) return made.Error!.ToResult<OldSiteArticleImportResponse>();
                article = made.Value;
                db.Articles.Add(article);
                existing[item.Slug] = article;
            }

            var replaced = article.Replace(item.Slug, item.PublishedOn, command.ShowOnSite || article.ShowOnSite, cover,
                texts, command.UpdatedBy, now);
            if (replaced.IsFailure) return replaced.Error!.ToResult<OldSiteArticleImportResponse>();
            await db.SaveChangesAsync(cancellationToken);
            foreach (var url in lost.Distinct())
                problems.Add(new OldSiteImportNote(item.Slug, item.Uk?.Title, $"{PhotoFailed}: {url} ({failed[url]})."));

            async Task<int?> PhotoAsync(string address)
            {
                var url = OldSiteArticlePhotos.Parse(address)!;
                var key = OldSiteArticlePhotos.SourceKey(url);
                if (imported.TryGetValue(key, out var id)) return id;
                if (!failed.ContainsKey(address))
                {
                    var download = await photos.DownloadAsync(url, key, OldSiteArticlePhotos.Folder, cancellationToken);
                    if (download.PhotoId is { } photoId)
                    {
                        downloaded++;
                        return imported[key] = photoId;
                    }

                    failed[address] = download.Error ?? "невідома причина";
                }

                lost.Add(address);
                return null;
            }

            // A picture that could not be brought is left out; the article keeps its text.
            async Task<ArticleTextInput> TextAsync(OldSiteArticleText text)
            {
                var ids = new Dictionary<string, int?>(StringComparer.Ordinal);
                foreach (var address in (text.Body ?? []).Where(x => x is { Kind: ArticleBlockKind.Image, ImageUrl: not null })
                         .Select(x => x.ImageUrl!).Distinct())
                    ids[address] = await PhotoAsync(address);
                return text.ToInput(address => ids.GetValueOrDefault(address));
            }
        }

        return new OldSiteArticleImportResponse(false, command.Items.Count, created, updated, kept, toDownload, downloaded,
            problems);
    }

    private static IEnumerable<string> PhotoUrls(OldSiteArticle item) =>
        new[] { item.CoverUrl }.Concat(new[] { item.Uk, item.Ru }.SelectMany(text => text?.Body ?? [])
            .Where(block => block is { Kind: ArticleBlockKind.Image }).Select(block => block.ImageUrl))
        .Where(url => url is not null).Select(url => url!);

    private static OldSiteImportNote Note(OldSiteArticle item, string reason) =>
        new(item.Slug, item.Uk?.Title ?? item.Ru?.Title, reason);
}
