using Content.Application.Abstractions;
using Content.Application.Articles.Save;
using Content.Application.Contracts;
using Content.Domain.Articles;
using Content.Domain.ProductPages;

namespace Content.Application.Articles.ImportOldSite;

/// <summary>A block of an old-site article; a picture is named by its address on the old site.</summary>
public sealed record OldSiteArticleBlock(
    ArticleBlockKind Kind,
    string? Text,
    string? Lead,
    IReadOnlyList<string>? Items,
    string? ImageUrl = null,
    string? Alt = null);

public sealed record OldSiteArticleText(
    string? Title,
    string? Excerpt,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<OldSiteArticleBlock>? Body)
{
    /// <summary>The texts as an article keeps them; <paramref name="photo"/> gives the library photo of a picture address.</summary>
    public ArticleTextInput ToInput(Func<string, int?> photo) => new(Title, Excerpt, SeoTitle, SeoDescription,
        (Body ?? []).Where(block => block is not null).Select(block => new ArticleBlock(block.Kind, block.Text,
            block.Lead, block.Items, block.Kind == ArticleBlockKind.Image ? photo(block.ImageUrl ?? string.Empty) : null,
            block.Alt)).ToArray());
}

/// <summary>An article of the old site: its address, date, the address of its cover and texts in both languages.</summary>
public sealed record OldSiteArticle(
    string Slug,
    DateOnly PublishedOn,
    string? CoverUrl,
    OldSiteArticleText? Uk,
    OldSiteArticleText? Ru);

/// <summary>
/// One-time transfer of the old-site articles with SEO, texts and pictures (CNT-004 SC-06). An article is matched by
/// its address. <paramref name="DryRun"/> only counts. <paramref name="Overwrite"/> replaces articles that already
/// exist; without it they are kept. Pictures are downloaded from the old site into the «Фото» library, each once.
/// <paramref name="ShowOnSite"/> switches transferred articles on; it never switches one off. Every article is saved
/// on its own, so a repeated run continues where the previous one stopped.
/// </summary>
public sealed record ImportOldSiteArticlesCommand(
    bool DryRun,
    bool ShowOnSite,
    bool Overwrite,
    IReadOnlyList<OldSiteArticle> Items,
    string UpdatedBy) : ICommand<Result<OldSiteArticleImportResponse>>
{
    public const int MaxItems = 200;
}

public sealed class ImportOldSiteArticlesCommandValidator : AbstractValidator<ImportOldSiteArticlesCommand>
{
    public ImportOldSiteArticlesCommandValidator()
    {
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Items).NotNull().Must(items => items is { Count: > 0 and <= ImportOldSiteArticlesCommand.MaxItems })
            .WithMessage($"Файл має містити від 1 до {ImportOldSiteArticlesCommand.MaxItems} статей.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Slug).Must(ProductPageRules.IsValidSlug).WithMessage("Стаття у файлі має некоректну адресу.");
            item.RuleFor(x => x.PublishedOn).Must(date => date.Year is >= 2000 and <= 2100)
                .WithMessage("Стаття у файлі має некоректну дату.");
            item.RuleFor(x => x.CoverUrl).Must(url => url is null || OldSiteArticlePhotos.IsValid(url))
                .WithMessage("Обкладинка статті у файлі має бути фото зі старого сайту (https, JPG, PNG або WebP).");
            item.RuleFor(x => x.Uk!).SetValidator(new OldSiteArticleTextValidator("Українська")).When(x => x.Uk is not null);
            item.RuleFor(x => x.Ru!).SetValidator(new OldSiteArticleTextValidator("Російська")).When(x => x.Ru is not null);
        }).When(x => x.Items is not null);
    }
}

public sealed class OldSiteArticleTextValidator : AbstractValidator<OldSiteArticleText>
{
    public OldSiteArticleTextValidator(string language)
    {
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Where(block => block is not null).All(block =>
                block.Kind != ArticleBlockKind.Image || OldSiteArticlePhotos.IsValid(block.ImageUrl)))
            .WithMessage($"{language}: фото в тексті має бути зі старого сайту (https, JPG, PNG або WebP).");
        // The same limits as a save from the editor; a picture counts as a block with a photo.
        RuleFor(x => x.ToInput(_ => 1)).SetValidator(new ArticleTextInputValidator(language)).OverridePropertyName("Text");
    }
}

public sealed class ImportOldSiteArticlesCommandHandler(
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
                article = new Article(item.Slug, item.PublishedOn, command.UpdatedBy, now);
                db.Articles.Add(article);
                existing[item.Slug] = article;
            }

            article.Replace(item.Slug, item.PublishedOn, command.ShowOnSite || article.ShowOnSite, cover, texts,
                command.UpdatedBy, now);
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
