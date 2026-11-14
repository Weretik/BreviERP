using Content.Domain.Articles;
using FluentValidation.TestHelper;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;
using Content.Application.Features.Articles.Queries.GetContentArticle;
using Content.Application.Features.Articles.Queries.GetContentArticles;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;

namespace UnitTests.Content.Articles;

/// <summary>One-time transfer of old-site articles with pictures (CNT-004 SC-06).</summary>
public sealed class ArticlesOldSiteImportTests
{
    private static readonly CancellationToken None = CancellationToken.None;
    private const string Cover = "https://brevi.com.ua/image/catalog/o_nas/33-3.jpg";
    private const string Helmets = "https://brevi.com.ua/image/catalog/kaska/kaski-kepki-2.jpg";
    private const string Lost = "https://brevi.com.ua/image/catalog/lost.png";

    private static OldSiteArticleText Text(string? title, string? image = null) => new(title, "Коротко.", "SEO", "Опис",
    [
        new OldSiteArticleBlock(ArticleBlockKind.Paragraph, "Текст статті.", null, null),
        .. image is null ? [] : new[] { new OldSiteArticleBlock(ArticleBlockKind.Image, null, null, null, image, "Каски") },
    ]);

    private static readonly OldSiteArticle[] File =
    [
        new("bezpeka-spivrobitnykiv", new DateOnly(2021, 7, 29), Cover, Text("Безпека співробітників"), Text("Безопасность сотрудников")),
        new("zahyst-golovy", new DateOnly(2022, 3, 1), Cover, Text("Захист голови", Helmets), Text("Защита головы", Helmets)),
        new("bez-nazvy", new DateOnly(2022, 3, 1), null, Text(" "), null),
        new("zahyst-golovy", new DateOnly(2022, 3, 1), null, Text("Повтор"), null),
    ];

    private static ImportOldSiteArticlesCommand Command(bool dryRun, bool show = false, bool overwrite = false,
        OldSiteArticle[]? items = null) => new(dryRun, show, overwrite, items ?? File, ContentTestHarness.Editor);

    [Test]
    public async Task Dry_run_counts_articles_and_pictures_and_writes_nothing()
    {
        using var h = new ContentTestHarness();

        var report = (await h.ImportArticles().Handle(Command(true), None)).Value;
        var list = (await h.Articles().Handle(new GetContentArticlesQuery(), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(report.DryRun, Is.True);
            Assert.That((report.Total, report.Created, report.Updated, report.Kept), Is.EqualTo((4, 2, 0, 0)));
            // The cover is shared by two articles and the helmets by two languages: each is brought once.
            Assert.That(report.PhotosToDownload, Is.EqualTo(2));
            Assert.That(report.PhotosDownloaded, Is.Zero);
            Assert.That(report.Problems.Select(x => x.Reason), Is.EquivalentTo(new[]
            {
                ImportOldSiteArticlesHandler.NoTitle, ImportOldSiteArticlesHandler.Duplicate,
            }));
            Assert.That(list.Items, Is.Empty);
            Assert.That(h.OldSitePhotos.Downloads, Is.Empty);
        });
    }

    [Test]
    public async Task Transfer_brings_each_picture_once_and_a_repeated_run_keeps_the_articles()
    {
        using var h = new ContentTestHarness();

        var first = (await h.ImportArticles().Handle(Command(false, show: true), None)).Value;
        var second = (await h.ImportArticles().Handle(Command(false), None)).Value;
        var list = (await h.Articles().Handle(new GetContentArticlesQuery(), None)).Value;
        var helmets = (await h.Article().Handle(
            new GetContentArticleQuery(list.Items.Single(x => x.Slug == "zahyst-golovy").Id), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That((first.Created, first.PhotosDownloaded), Is.EqualTo((2, 2)));
            Assert.That((second.Created, second.Kept, second.PhotosToDownload, second.PhotosDownloaded),
                Is.EqualTo((0, 2, 0, 0)));
            Assert.That(h.OldSitePhotos.Downloads.Select(x => (x.Key, x.Folder)), Is.EquivalentTo(new[]
            {
                ("old-site:catalog/o_nas/33-3.jpg", "Старий сайт/Статті"),
                ("old-site:catalog/kaska/kaski-kepki-2.jpg", "Старий сайт/Статті"),
            }));
            Assert.That(list.Items.Select(x => x.Slug), Is.EqualTo(new[] { "zahyst-golovy", "bezpeka-spivrobitnykiv" }));
            Assert.That(list.Items.All(x => x.ShowOnSite && x.CoverUrl is not null && x.HasSeo), Is.True);
            Assert.That(helmets.Article.PublishedOn, Is.EqualTo(new DateOnly(2022, 3, 1)));
            Assert.That(helmets.Uk.Body.Single(x => x.Kind == ArticleBlockKind.Image).PhotoId,
                Is.EqualTo(helmets.Ru.Body.Single(x => x.Kind == ArticleBlockKind.Image).PhotoId));
            Assert.That(helmets.Uk.Body.Single(x => x.Kind == ArticleBlockKind.Image).Alt, Is.EqualTo("Каски"));
            Assert.That(helmets.Photos, Has.Count.EqualTo(2));
            Assert.That(helmets.UpdatedBy, Is.EqualTo(ContentTestHarness.Editor));
        });
    }

    [Test]
    public async Task Existing_article_is_kept_unless_overwrite_and_is_never_switched_off()
    {
        using var h = new ContentTestHarness();
        await h.ImportArticles().Handle(Command(false, show: true), None);
        OldSiteArticle[] changed = [File[0] with { Uk = Text("Безпека працівників"), CoverUrl = null }];

        var kept = (await h.ImportArticles().Handle(Command(false, items: changed), None)).Value;
        var titleKept = (await h.Articles().Handle(new GetContentArticlesQuery(), None)).Value.Items
            .Single(x => x.Slug == changed[0].Slug).TitleUk;
        var replaced = (await h.ImportArticles().Handle(Command(false, overwrite: true, items: changed), None)).Value;
        var row = (await h.Articles().Handle(new GetContentArticlesQuery(), None)).Value.Items
            .Single(x => x.Slug == changed[0].Slug);

        Assert.Multiple(() =>
        {
            Assert.That((kept.Kept, kept.Updated), Is.EqualTo((1, 0)));
            Assert.That(titleKept, Is.EqualTo("Безпека співробітників"));
            Assert.That((replaced.Kept, replaced.Updated), Is.EqualTo((0, 1)));
            Assert.That(row.TitleUk, Is.EqualTo("Безпека працівників"));
            Assert.That(row.ShowOnSite, Is.True);
            Assert.That(row.CoverUrl, Is.Null);
        });
    }

    [Test]
    public async Task Picture_that_fails_is_left_out_with_a_reason_and_the_article_is_saved()
    {
        using var h = new ContentTestHarness();
        h.OldSitePhotos.Broken.Add(Lost);
        OldSiteArticle[] items = [new("stattya-z-foto", new DateOnly(2023, 1, 1), Lost, Text("Стаття", Lost), null)];

        var report = (await h.ImportArticles().Handle(Command(false, items: items), None)).Value;
        var list = (await h.Articles().Handle(new GetContentArticlesQuery(), None)).Value;
        var article = (await h.Article().Handle(new GetContentArticleQuery(list.Items[0].Id), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That((report.Created, report.PhotosToDownload, report.PhotosDownloaded), Is.EqualTo((1, 1, 0)));
            Assert.That(report.Problems, Has.Count.EqualTo(1));
            Assert.That(report.Problems[0].Reason, Does.StartWith(ImportOldSiteArticlesHandler.PhotoFailed)
                .And.Contain("lost.png").And.Contain("404"));
            Assert.That(h.OldSitePhotos.Downloads, Has.Count.EqualTo(1));
            Assert.That(article.CoverPhotoId, Is.Null);
            Assert.That(article.Uk.Body.Select(x => x.Kind), Is.EqualTo(new[] { ArticleBlockKind.Paragraph }));
        });
    }

    [TestCase("https://brevi.com.ua/image/catalog/a.jpg", true)]
    [TestCase("https://www.brevi.com.ua/image/catalog/a.WEBP", true)]
    [TestCase("http://brevi.com.ua/image/catalog/a.jpg", false)]
    [TestCase("https://evil.example/image/catalog/a.jpg", false)]
    [TestCase("https://brevi.com.ua.evil.example/a.jpg", false)]
    [TestCase("https://user@brevi.com.ua/a.jpg", false)]
    [TestCase("https://brevi.com.ua:8443/a.jpg", false)]
    [TestCase("https://brevi.com.ua/index.php?route=a.jpg", false)]
    [TestCase("/image/catalog/a.jpg", false)]
    [TestCase(null, false)]
    public void Only_pictures_of_the_old_site_are_accepted(string? url, bool valid) =>
        Assert.That(OldSiteArticlePhotos.IsValid(url), Is.EqualTo(valid));

    [Test]
    public void Validator_checks_the_file()
    {
        var validator = new ImportOldSiteArticlesValidator();
        OldSiteArticle Article(string slug = "stattya", string? cover = null, string? image = null, int year = 2022) =>
            new(slug, new DateOnly(year, 1, 1), cover, Text("Стаття", image), null);

        Assert.Multiple(() =>
        {
            Assert.That(validator.TestValidate(Command(true)).IsValid, Is.True);
            Assert.That(validator.TestValidate(Command(true, items: [])).IsValid, Is.False);
            Assert.That(validator.TestValidate(Command(true, items: [Article("Не адреса")])).IsValid, Is.False);
            Assert.That(validator.TestValidate(Command(true, items: [Article(year: 1990)])).IsValid, Is.False);
            Assert.That(validator.TestValidate(Command(true, items: [Article(cover: "https://evil.example/a.jpg")])).IsValid, Is.False);
            Assert.That(validator.TestValidate(Command(true, items: [Article(image: "http://brevi.com.ua/a.jpg")])).IsValid, Is.False);
            Assert.That(validator.TestValidate(Command(true, items:
                [Article() with { Uk = new OldSiteArticleText(new string('а', ArticleRules.TitleMaxLength + 1), null, null, null, null) }]))
                .IsValid, Is.False);
        });
    }
}
