using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Content.Domain.Articles;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using YamlDotNet.RepresentationModel;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Queries.GetPublicArticle;
using Content.Application.Features.Articles.Queries.GetPublicArticles;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Public;

namespace UnitTests.Content;

/// <summary>Articles of the public site: list and page by address, without sign-in (CNT-004 SC-07, R-07).</summary>
public sealed class PublicArticlesTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    private static readonly CancellationToken None = CancellationToken.None;

    private static ArticleTextInput Text(string title, int? photoId = null, bool withBody = true) =>
        new(title, $"Коротко: {title}", $"SEO {title}", $"Опис {title}", withBody
            ?
            [
                new ArticleBlock(ArticleBlockKind.Heading, "Тканина", null, null),
                new ArticleBlock(ArticleBlockKind.Paragraph, "Шиємо **самі**.", "Важливо:", null),
                new ArticleBlock(ArticleBlockKind.List, null, null, ["щільність"]),
                new ArticleBlock(ArticleBlockKind.Image, null, null, null, photoId ?? 7, "Костюм у цеху"),
            ]
            : null);

    private static async Task Add(ContentTestHarness h, string slug, DateOnly day, bool show = true,
        int? cover = null, ArticleTextInput? ru = null, int? photoId = null)
    {
        var result = await h.SaveArticle().Handle(new SaveContentArticleCommand(null, 0, slug, day, show, cover,
            Text($"Стаття {slug}", photoId), ru, ContentTestHarness.Editor), None);
        Assert.That(result.IsSuccess, Is.True, string.Join(' ', result.Errors));
    }

    private static ContentTestHarness Site()
    {
        var h = new ContentTestHarness();
        h.AddPhoto(5);
        h.AddPhoto(7, withThumbnail: false);
        return h;
    }

    [Test]
    public async Task List_has_only_shown_articles_the_newest_first_with_cover_and_language()
    {
        using var h = Site();
        await Add(h, "stara", new DateOnly(2021, 7, 28), cover: 5);
        await Add(h, "nova", new DateOnly(2025, 3, 1), ru: Text("Новая статья"));
        await Add(h, "prykhovana", new DateOnly(2026, 1, 1), show: false);

        var list = (await new GetPublicArticlesHandler(h.PublicArticles())
            .Handle(new GetPublicArticlesQuery("ru"), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(list.Items.Select(x => (x.Slug, x.Language, x.Title, x.HasRussian)), Is.EqualTo(new[]
            {
                ("nova", "ru", "Новая статья", true),
                ("stara", "uk", "Стаття stara", false),
            }));
            Assert.That(list.Items[1].Cover,
                Is.EqualTo(new PublicArticlePhoto("https://cdn.test/photos/5.jpg", "https://cdn.test/photos/5-small.webp", 1200, 800)));
            Assert.That(list.Items[0].Cover, Is.Null);
            Assert.That(list.Items[1].PublishedOn, Is.EqualTo(new DateOnly(2021, 7, 28)));
        });
    }

    [Test]
    public async Task Page_has_seo_and_blocks_with_photos_and_falls_back_to_ukrainian()
    {
        using var h = Site();
        await Add(h, "tkanyny", new DateOnly(2024, 5, 1), cover: 5, ru: Text("Ткани", withBody: false));

        var handler = new GetPublicArticleHandler(h.PublicArticles());
        var uk = (await handler.Handle(new GetPublicArticleQuery("tkanyny", "uk"), None)).Value;
        var ru = (await handler.Handle(new GetPublicArticleQuery("tkanyny", "ru"), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That((uk.Language, uk.Title, uk.SeoTitle, uk.SeoDescription, uk.Excerpt),
                Is.EqualTo(("uk", "Стаття tkanyny", "SEO Стаття tkanyny", "Опис Стаття tkanyny", "Коротко: Стаття tkanyny")));
            Assert.That(uk.Body.Select(x => x.Kind), Is.EqualTo(new[]
            {
                ArticleBlockKind.Heading, ArticleBlockKind.Paragraph, ArticleBlockKind.List, ArticleBlockKind.Image,
            }));
            Assert.That((uk.Body[1].Text, uk.Body[1].Lead), Is.EqualTo(("Шиємо **самі**.", "Важливо:")));
            Assert.That(uk.Body[2].Items, Is.EqualTo(new[] { "щільність" }));
            Assert.That((uk.Body[3].Photo?.Url, uk.Body[3].Photo?.ThumbnailUrl, uk.Body[3].Alt),
                Is.EqualTo(("https://cdn.test/photos/7.jpg", (string?)null, "Костюм у цеху")));
            Assert.That(uk.Cover?.Url, Is.EqualTo("https://cdn.test/photos/5.jpg"));
            // The Russian text has a title but no body, so the page stays Ukrainian.
            Assert.That((ru.Language, ru.Title, ru.HasRussian), Is.EqualTo(("uk", "Стаття tkanyny", false)));
        });
    }

    [Test]
    public async Task Photo_deleted_from_the_library_is_left_out()
    {
        using var h = Site();
        await Add(h, "bez-foto", new DateOnly(2024, 5, 1), cover: 5);
        h.Media.Photos.Clear();

        var page = (await new GetPublicArticleHandler(h.PublicArticles())
            .Handle(new GetPublicArticleQuery("bez-foto", "uk"), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(page.Cover, Is.Null);
            Assert.That(page.Body.Select(x => x.Kind), Has.No.Member(ArticleBlockKind.Image));
            Assert.That(page.Body, Has.Count.EqualTo(3));
        });
    }

    [Test]
    public async Task Hidden_and_unknown_addresses_are_not_found()
    {
        using var h = Site();
        await Add(h, "prykhovana", new DateOnly(2024, 5, 1), show: false);
        var handler = new GetPublicArticleHandler(h.PublicArticles());

        Assert.Multiple(async () =>
        {
            Assert.That((await handler.Handle(new GetPublicArticleQuery("prykhovana", "uk"), None)).Status,
                Is.EqualTo(Ardalis.Result.ResultStatus.NotFound));
            Assert.That((await handler.Handle(new GetPublicArticleQuery("nemaye", "uk"), None)).Status,
                Is.EqualTo(Ardalis.Result.ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task Articles_are_kept_for_a_short_time_so_a_change_reaches_the_site_after_it()
    {
        using var h = Site();
        await Add(h, "persha", new DateOnly(2024, 5, 1));
        using var cache = new MemoryCache(new MemoryCacheOptions());

        var before = await h.PublicArticles(cache).GetAsync(None);
        await Add(h, "druha", new DateOnly(2024, 6, 1));
        var kept = await h.PublicArticles(cache).GetAsync(None);
        cache.Compact(1);
        var after = await h.PublicArticles(cache).GetAsync(None);

        Assert.Multiple(() =>
        {
            Assert.That(kept, Is.SameAs(before));
            Assert.That(after.Articles.Select(x => x.Slug), Is.EqualTo(new[] { "druha", "persha" }));
            Assert.That(PublicArticlesSource.Lifetime, Is.LessThanOrEqualTo(TimeSpan.FromMinutes(5)));
        });
    }

    [Test]
    public void Queries_refuse_a_wrong_language_and_an_empty_address()
    {
        var list = new GetPublicArticlesValidator();
        var one = new GetPublicArticleValidator();
        Assert.Multiple(() =>
        {
            list.TestValidate(new GetPublicArticlesQuery("uk")).ShouldNotHaveAnyValidationErrors();
            list.TestValidate(new GetPublicArticlesQuery("en")).ShouldHaveValidationErrorFor(x => x.Language);
            one.TestValidate(new GetPublicArticleQuery("", "uk")).ShouldHaveValidationErrorFor(x => x.Slug);
            one.TestValidate(new GetPublicArticleQuery("a", "de")).ShouldHaveValidationErrorFor(x => x.Language);
        });
    }

    [TestCase(typeof(PublicArticlePhoto), "PublicArticlePhoto")]
    [TestCase(typeof(PublicArticleCard), "PublicArticleCard")]
    [TestCase(typeof(PublicArticleList), "PublicArticleList")]
    [TestCase(typeof(PublicArticleBlock), "PublicArticleBlock")]
    [TestCase(typeof(PublicArticle), "PublicArticle")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_open_to_everyone_and_read_only()
    {
        var controller = "PublicArticles";
        var methods = MappedEndpoints.Of(Map, controller)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(methods.All(x => x.Anonymous), Is.True);
            Assert.That(methods.All(x => x.Method == "GET"), Is.True);
            Assert.That(methods.Select(x => x.Name), Is.EquivalentTo(Operations()));
            Assert.That(methods, Has.Length.EqualTo(2));
        });
    }

    [Test]
    public void Answers_carry_nothing_internal()
    {
        string[] forbidden = ["id", "updatedBy", "revision", "showOnSite", "problem"];
        var names = new[] { typeof(PublicArticleCard), typeof(PublicArticle), typeof(PublicArticleBlock), typeof(PublicArticlePhoto) }
            .SelectMany(x => x.GetProperties()).Select(x => x.Name).ToArray();

        Assert.That(names.Where(name => forbidden.Any(x => name.Equals(x, StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("Id", StringComparison.Ordinal))), Is.Empty);
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/content/content-public-articles.openapi.yaml"));
        var yaml = new YamlStream();
        yaml.Load(reader);
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    private static YamlMappingNode Schemas() =>
        (YamlMappingNode)((YamlMappingNode)Root()["components"])["schemas"];

    private static IEnumerable<string> Operations() =>
        ((YamlMappingNode)Root()["paths"]).Children.Values.Cast<YamlMappingNode>()
        .SelectMany(path => path.Children.Values.OfType<YamlMappingNode>())
        .Where(operation => operation.Children.ContainsKey("operationId"))
        .Select(operation => ((YamlScalarNode)operation["operationId"]).Value!);
}
