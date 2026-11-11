using Ardalis.Result;
using Content.Application.Articles.Delete;
using Content.Application.Articles.GetList;
using Content.Application.Articles.GetOne;
using Content.Application.Articles.Save;
using Content.Application.Articles.SetVisibility;
using Content.Application.Articles.Shared;
using Content.Application.Contracts;
using Content.Domain.Articles;
using FluentValidation.TestHelper;

namespace UnitTests.Content;

/// <summary>«Контент → Статті»: list, editor, save, photos, «на сайті» and delete (CNT-004 SC-01..SC-05).</summary>
public sealed class ArticlesTests
{
    private static readonly CancellationToken None = CancellationToken.None;
    private static readonly DateOnly Day = new(2026, 10, 20);

    private static ArticleTextInput Text(string? title, string? seoTitle = null, string? seoDescription = null,
        int? photoId = null, bool withBody = true) =>
        new(title, "  Як  обрати спецодяг. ", seoTitle, seoDescription, withBody
            ?
            [
                new ArticleBlock(ArticleBlockKind.Heading, "Тканина", "зайве", null),
                new ArticleBlock(ArticleBlockKind.Paragraph, "  Шиємо  самі.  ", "Важливо:", null),
                new ArticleBlock(ArticleBlockKind.List, null, null, ["щільність", " "]),
                new ArticleBlock(ArticleBlockKind.Paragraph, "  ", null, null),
                new ArticleBlock(ArticleBlockKind.Image, "зайве", null, null, photoId, "  Костюм  у цеху "),
            ]
            : null);

    private static SaveContentArticleCommand New(string title, string? slug = null, DateOnly? day = null,
        bool show = false, int? cover = null, int? photoId = null, ArticleTextInput? ru = null) =>
        new(null, 0, slug, day ?? Day, show, cover, Text(title, photoId: photoId), ru, ContentTestHarness.Editor);

    private static async Task<ArticleResponse> Add(ContentTestHarness h, string title, string? slug = null,
        DateOnly? day = null, bool show = false, int? cover = null, int? photoId = null, ArticleTextInput? ru = null)
    {
        var result = await h.SaveArticle().Handle(New(title, slug, day, show, cover, photoId, ru), None);
        Assert.That(result.IsSuccess, Is.True, string.Join(' ', result.Errors));
        return result.Value;
    }

    [Test]
    public async Task New_article_is_hidden_gets_an_address_from_its_title_and_clean_blocks()
    {
        using var h = new ContentTestHarness();
        h.AddPhoto(7);

        var article = await Add(h, "  Як обрати  спецодяг ", photoId: 7);

        Assert.Multiple(() =>
        {
            Assert.That(article.Article.Slug, Is.EqualTo("yak-obraty-spetsodyah"));
            Assert.That(article.Article.ShowOnSite, Is.False);
            Assert.That(article.Article.PublishedOn, Is.EqualTo(Day));
            Assert.That(article.Article.TitleUk, Is.EqualTo("Як обрати спецодяг"));
            Assert.That(article.Article.HasUkrainianText, Is.True);
            Assert.That(article.Article.HasRussianText, Is.False);
            Assert.That(article.Revision, Is.EqualTo(1));
            Assert.That(article.UpdatedBy, Is.EqualTo(ContentTestHarness.Editor));
            Assert.That(article.Uk.Excerpt, Is.EqualTo("Як обрати спецодяг."));
            Assert.That(article.Uk.Body, Is.EqualTo(new[]
            {
                new ArticleBlock(ArticleBlockKind.Heading, "Тканина", null, null),
                new ArticleBlock(ArticleBlockKind.Paragraph, "Шиємо самі.", "Важливо:", null),
                new ArticleBlock(ArticleBlockKind.List, null, null, ["щільність"]),
                new ArticleBlock(ArticleBlockKind.Image, null, null, null, 7, "Костюм у цеху"),
            }).Using<ArticleBlock>(Same));
            Assert.That(article.Ru.Body, Is.Empty);
        });
    }

    [Test]
    public async Task Same_title_gets_a_numbered_address_and_a_taken_address_is_refused()
    {
        using var h = new ContentTestHarness();
        await Add(h, "Зимовий одяг");

        var again = await Add(h, "Зимовий одяг");
        var taken = await h.SaveArticle().Handle(New("Інша стаття", "zymovyy-odyah"), None);

        Assert.Multiple(() =>
        {
            Assert.That(again.Article.Slug, Is.EqualTo($"{again.Article.Slug[..^2]}-2"));
            Assert.That(again.Article.Slug, Does.EndWith("-2"));
            Assert.That(taken.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(taken.Errors, Does.Contain(ArticleMessages.SlugTaken));
        });
    }

    [Test]
    public async Task Cover_and_body_photos_must_be_in_the_library()
    {
        using var h = new ContentTestHarness();
        h.AddPhoto(1);
        h.AddPhoto(2, withThumbnail: false);

        var missingCover = await h.SaveArticle().Handle(New("Стаття", cover: 99), None);
        var missingPhoto = await h.SaveArticle().Handle(New("Стаття", cover: 1, photoId: 98), None);
        var saved = await Add(h, "Стаття", cover: 1, photoId: 2);
        var list = (await h.Articles().Handle(new GetContentArticlesQuery(), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(missingCover.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(missingCover.Errors, Does.Contain(ArticleMessages.PhotoNotFound));
            Assert.That(missingPhoto.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(list.Items, Has.Count.EqualTo(1));
            Assert.That(saved.CoverPhotoId, Is.EqualTo(1));
            Assert.That(saved.Photos.Select(x => x.Id), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(saved.Article.CoverUrl, Is.EqualTo("https://cdn.test/photos/1-small.webp"));
        });
    }

    [Test]
    public async Task Editor_leaves_out_a_photo_deleted_from_the_library_and_the_row_falls_back_to_the_full_photo()
    {
        using var h = new ContentTestHarness();
        h.AddPhoto(1, withThumbnail: false);
        h.AddPhoto(2);
        var saved = await Add(h, "Стаття", cover: 1, photoId: 2);
        h.Media.Photos.RemoveAll(x => x.Id == 2);

        var one = (await h.Article().Handle(new GetContentArticleQuery(saved.Article.Id), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(one.Article.CoverUrl, Is.EqualTo("https://cdn.test/photos/1.jpg"));
            Assert.That(one.Photos.Select(x => x.Id), Is.EqualTo(new[] { 1 }));
            Assert.That(one.Uk.Body.Any(x => x.PhotoId == 2), Is.True);
        });
    }

    [Test]
    public async Task Save_replaces_the_article_keeps_the_address_and_needs_the_shown_revision()
    {
        using var h = new ContentTestHarness();
        var created = await Add(h, "Стаття перша", ru: Text("Статья первая", "SEO", "Опис"));
        h.Time.Advance(TimeSpan.FromHours(1));

        var saved = await h.SaveArticle().Handle(new SaveContentArticleCommand(created.Article.Id, created.Revision, " ",
            new DateOnly(2026, 11, 1), true, null, Text("Стаття оновлена", "SEO uk", "Опис uk", withBody: false),
            new ArticleTextInput(" ", null, null, null, null), "director@brevi.test"), None);
        var stale = await h.SaveArticle().Handle(new SaveContentArticleCommand(created.Article.Id, created.Revision, null,
            Day, false, null, Text("Ще раз"), null, ContentTestHarness.Editor), None);
        var missing = await h.SaveArticle().Handle(new SaveContentArticleCommand(404, 1, null, Day, false, null,
            Text("Немає"), null, ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(saved.IsSuccess, Is.True, string.Join(' ', saved.Errors));
            Assert.That(saved.Value.Article.Slug, Is.EqualTo(created.Article.Slug));
            Assert.That(saved.Value.Article.TitleUk, Is.EqualTo("Стаття оновлена"));
            Assert.That(saved.Value.Article.TitleRu, Is.Null);
            Assert.That(saved.Value.Article.ShowOnSite, Is.True);
            Assert.That(saved.Value.Article.PublishedOn, Is.EqualTo(new DateOnly(2026, 11, 1)));
            Assert.That(saved.Value.Article.HasUkrainianText, Is.False);
            Assert.That(saved.Value.Article.HasSeo, Is.False);
            Assert.That(saved.Value.Revision, Is.EqualTo(2));
            Assert.That(saved.Value.UpdatedBy, Is.EqualTo("director@brevi.test"));
            Assert.That(saved.Value.Article.UpdatedAtUtc, Is.EqualTo(h.Time.GetUtcNow()));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(stale.Errors, Does.Contain(ArticleMessages.Stale));
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task Seo_is_ready_only_when_both_languages_have_title_and_description()
    {
        using var h = new ContentTestHarness();

        var half = await Add(h, "Стаття А", ru: Text("Статья А"));
        var full = await h.SaveArticle().Handle(new SaveContentArticleCommand(null, 0, "stattya-b", Day, false, null,
            Text("Стаття Б", "SEO", "Опис"), Text("Статья Б", "SEO", "Описание"), ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(half.Article.HasSeo, Is.False);
            Assert.That(half.Article.HasRussianText, Is.True);
            Assert.That(full.Value.Article.HasSeo, Is.True);
        });
    }

    [Test]
    public async Task List_shows_the_newest_first()
    {
        using var h = new ContentTestHarness();
        await Add(h, "Стара стаття", day: new DateOnly(2024, 1, 5));
        await Add(h, "Нова стаття", day: new DateOnly(2026, 11, 1));
        await Add(h, "Ще одна нова", day: new DateOnly(2026, 11, 1));

        var list = (await h.Articles().Handle(new GetContentArticlesQuery(), None)).Value;

        Assert.That(list.Items.Select(x => x.TitleUk), Is.EqualTo(new[] { "Ще одна нова", "Нова стаття", "Стара стаття" }));
    }

    [Test]
    public async Task Visibility_switch_raises_the_revision_only_when_it_changes()
    {
        using var h = new ContentTestHarness();
        var created = await Add(h, "Стаття");

        var on = await h.ArticleVisibility().Handle(
            new SetContentArticleVisibilityCommand(created.Article.Id, true, ContentTestHarness.Editor), None);
        var same = await h.ArticleVisibility().Handle(
            new SetContentArticleVisibilityCommand(created.Article.Id, true, ContentTestHarness.Editor), None);
        var missing = await h.ArticleVisibility().Handle(
            new SetContentArticleVisibilityCommand(404, true, ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(on.Value.ShowOnSite, Is.True);
            Assert.That(on.Value.Revision, Is.EqualTo(2));
            Assert.That(same.Value.Revision, Is.EqualTo(2));
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task Delete_removes_the_article_with_its_texts()
    {
        using var h = new ContentTestHarness();
        var created = await Add(h, "Стаття", ru: Text("Статья"));

        var deleted = await h.DeleteArticle().Handle(new DeleteContentArticleCommand(created.Article.Id), None);
        var again = await h.DeleteArticle().Handle(new DeleteContentArticleCommand(created.Article.Id), None);
        var one = await h.Article().Handle(new GetContentArticleQuery(created.Article.Id), None);

        Assert.Multiple(() =>
        {
            Assert.That(deleted.IsSuccess, Is.True);
            Assert.That(again.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(one.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(h.Open().Set<ArticleText>().Count(), Is.Zero);
        });
    }

    [Test]
    public void Validator_needs_a_ukrainian_title_a_valid_address_and_a_photo_in_picture_blocks()
    {
        var validator = new SaveContentArticleCommandValidator();
        var picture = new ArticleTextInput("Стаття", null, null, null,
            [new ArticleBlock(ArticleBlockKind.Image, null, null, null, null, new string('а', ArticleRules.AltMaxLength + 1))]);

        var noTitle = validator.TestValidate(New(" "));
        var badSlug = validator.TestValidate(New("Стаття", "Погана адреса"));
        var badDate = validator.TestValidate(New("Стаття", day: new DateOnly(1990, 1, 1)));
        var badPicture = validator.TestValidate(new SaveContentArticleCommand(null, 0, null, Day, false, null, picture,
            null, ContentTestHarness.Editor));
        var longTitle = validator.TestValidate(New(new string('а', ArticleRules.TitleMaxLength + 1)));
        var good = validator.TestValidate(New("Стаття", "stattya", photoId: 3));

        Assert.Multiple(() =>
        {
            Assert.That(noTitle.Errors.Select(x => x.ErrorMessage), Does.Contain(ArticleMessages.TitleRequired));
            Assert.That(badSlug.Errors.Select(x => x.ErrorMessage), Does.Contain(ArticleMessages.SlugInvalid));
            Assert.That(badDate.IsValid, Is.False);
            Assert.That(badPicture.Errors.Select(x => x.ErrorMessage),
                Has.Some.Contains("не обрано фото").And.Some.Contains("підпис фото"));
            Assert.That(longTitle.IsValid, Is.False);
            Assert.That(good.IsValid, Is.True, string.Join(' ', good.Errors.Select(x => x.ErrorMessage)));
        });
    }

    private static bool Same(ArticleBlock a, ArticleBlock b) =>
        a.Kind == b.Kind && a.Text == b.Text && a.Lead == b.Lead && a.PhotoId == b.PhotoId && a.Alt == b.Alt
        && (a.Items ?? []).SequenceEqual(b.Items ?? []);
}
