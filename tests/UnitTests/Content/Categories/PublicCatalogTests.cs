using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Ardalis.Result;
using Content.Application.Abstractions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using YamlDotNet.RepresentationModel;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProductSlugs;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;
using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;
using Content.Application.Features.Catalog.Shared.Links;
using Content.Application.Features.Catalog.Shared.Products;

namespace UnitTests.Content.Categories;

/// <summary>The catalog of the public site: tree, cards with «від», product by address, addresses (CNT-002 SC-06).</summary>
public sealed class PublicCatalogTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    private static readonly CancellationToken None = CancellationToken.None;

    private static async Task ShowAsync(ContentTestHarness h, int productId, string slug, bool russian = false,
        bool showOnSite = true) =>
        Assert.That((await h.Save().Handle(new SaveContentProductCommand(productId, 0, slug, showOnSite,
            ContentTestHarness.Text(), russian ? ContentTestHarness.Text("Костюм рабочий «Лидер»") : null,
            ContentTestHarness.Editor), None)).IsSuccess, Is.True);

    private static ContentTestHarness Site()
    {
        var h = new ContentTestHarness();
        h.Catalog.Sections.AddRange([
            new ContentCatalogSection(11, "Sewing", "Куртки", true),
            new ContentCatalogSection(10, "Sewing", "Костюми", true),
            new ContentCatalogSection(12, "Sewing", "Жилети", true),
            new ContentCatalogSection(20, "Ppe", "Захист голови", true),
        ]);
        return h;
    }

    [Test]
    public async Task Tree_has_only_types_and_sections_with_products_on_the_site_in_reference_order()
    {
        using var h = Site();
        h.AddSewing();
        h.AddSewing(2, "Куртка Зима", "MOD-002", sectionId: 11, sectionName: "Куртки");
        h.AddSewing(3, "Костюм Прихований", "MOD-003");
        h.AddSewing(4, "Костюм Вимкнений", "MOD-004", active: false);
        h.AddPpe();
        await ShowAsync(h, 1, "kostyum-lider");
        await ShowAsync(h, 2, "kurtka-zyma");
        await ShowAsync(h, 3, "kostyum-prykhovanyy", showOnSite: false);
        await ShowAsync(h, 4, "kostyum-vymknenyy");

        var tree = (await new GetPublicCatalogTreeHandler(h.Site()).Handle(new(), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(tree.Types.Select(x => x.Type), Is.EqualTo(new[] { "Sewing" }));
            Assert.That(tree.Types[0].ProductCount, Is.EqualTo(2));
            Assert.That(tree.Types[0].Sections.Select(x => (x.Id, x.Slug, x.Name, x.ProductCount)), Is.EqualTo(new[]
            {
                (11, "kurtky", "Куртки", 1),
                (10, "kostyumy", "Костюми", 1),
            }));
        });
    }

    [Test]
    public async Task Sections_with_the_same_name_get_different_addresses()
    {
        using var h = Site();
        h.Catalog.Sections.Add(new ContentCatalogSection(21, "Ppe", "Костюми", true));
        h.AddSewing();
        var ppe = h.AddPpe();
        h.Catalog.Products[1] = ppe with { SectionId = 21 };
        await ShowAsync(h, 1, "kostyum-lider");
        await ShowAsync(h, 50, "kaska");

        var tree = (await new GetPublicCatalogTreeHandler(h.Site()).Handle(new(), None)).Value;

        Assert.That(tree.Types.SelectMany(x => x.Sections).Select(x => x.Slug),
            Is.EqualTo(new[] { "kostyumy", "kostyumy-21" }));
    }

    [Test]
    public async Task Cards_are_filtered_and_paged_and_show_the_lowest_large_wholesale_price()
    {
        using var h = Site();
        var first = h.AddSewing();
        h.Catalog.Products[0] = first with { MainPhotoPreviewUrl = "https://cdn.test/1-small.jpg" };
        h.AddSewing(2, "Куртка Зима", "MOD-002", sectionId: 11, sectionName: "Куртки");
        h.AddPpe();
        await ShowAsync(h, 1, "kostyum-lider", russian: true);
        await ShowAsync(h, 2, "kurtka-zyma");
        await ShowAsync(h, 50, "kaska");
        var handler = new GetPublicCatalogProductsHandler(h.Site());

        var all = (await handler.Handle(new(1, 2, "ru"), None)).Value;
        var ppe = (await handler.Handle(new(1, 24, "uk", "Ppe"), None)).Value;
        var jackets = (await handler.Handle(new(1, 24, "uk", null, 11), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(all.Total, Is.EqualTo(3));
            Assert.That(all.Items.Select(x => x.Slug), Is.EqualTo(new[] { "kostyum-lider", "kurtka-zyma" }));
            var card = all.Items[0];
            Assert.That((card.Slug, card.Type, card.Code, card.Name, card.SectionId, card.PhotoUrl, card.PriceFrom),
                Is.EqualTo(("kostyum-lider", "Sewing", "MOD-001", "Костюм рабочий «Лидер»", (int?)10,
                    "https://cdn.test/1-small.jpg", (decimal?)1300m)));
            Assert.That(all.Items[0].Fabrics, Is.EqualTo(new[] { "Грета", "Саржа" }));
            Assert.That(ppe.Items.Single().Fabrics, Is.Empty);
            // No Russian texts: the Ukrainian name, and the full photo when there is no small copy.
            Assert.That(all.Items[1].Name, Is.EqualTo("Костюм робочий «Лідер»"));
            Assert.That(all.Items[1].PhotoUrl, Is.EqualTo("https://cdn.test/1.jpg"));
            Assert.That(ppe.Items.Single().PriceFrom, Is.Null);
            Assert.That(jackets.Items.Single().Slug, Is.EqualTo("kurtka-zyma"));
        });
    }

    [Test]
    public async Task Card_lists_each_active_fabric_once()
    {
        using var h = Site();
        var product = h.AddSewing();
        var price = new ContentPriceTiers(1500m, 1400m, 1300m);
        h.Catalog.Products[0] = product with
        {
            Variants =
            [
                .. product.Variants,
                new ContentCatalogVariant(301, "MOD-001-C", true, " грета ", null, price),
                new ContentCatalogVariant(302, "MOD-001-D", false, "Оксфорд", null, price),
                new ContentCatalogVariant(303, "MOD-001-E", true, null, null, price),
            ],
        };
        await ShowAsync(h, 1, "kostyum-lider");

        var page = (await new GetPublicCatalogProductsHandler(h.Site()).Handle(new(1, 24, "uk"), None)).Value;

        Assert.That(page.Items.Single().Fabrics, Is.EqualTo(new[] { "Грета", "Саржа" }));
    }

    [Test]
    public async Task Product_by_address_has_texts_photos_and_active_fabrics_with_prices()
    {
        using var h = Site();
        var product = h.AddSewing();
        h.Catalog.Products[0] = product with
        {
            Variants = [.. product.Variants, new ContentCatalogVariant(301, "MOD-001-C", false, "Стара", null, null)],
        };
        h.Catalog.Photos[1] = [new("https://cdn.test/1.jpg", "https://cdn.test/1-small.jpg", true), new("https://cdn.test/2.jpg", null, false)];
        await ShowAsync(h, 1, "kostyum-lider");
        var handler = new GetPublicCatalogProductHandler(h.Site(), h.Catalog);

        var uk = (await handler.Handle(new("kostyum-lider", "uk"), None)).Value;
        var ru = (await handler.Handle(new("kostyum-lider", "ru"), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(uk.Name, Is.EqualTo("Костюм робочий «Лідер»"));
            Assert.That(uk.Language, Is.EqualTo("uk"));
            Assert.That(uk.Section, Is.EqualTo(new PublicCatalogSectionLink(10, "kostyumy", "Костюми")));
            Assert.That(uk.SeoTitle, Is.EqualTo("Купити костюм Лідер"));
            Assert.That(uk.Summary, Has.Count.EqualTo(1));
            Assert.That(uk.Description, Has.Count.EqualTo(3));
            Assert.That(uk.Specs.Single().Rows, Has.Count.EqualTo(1));
            Assert.That(uk.Photos.Select(x => x.Url), Is.EqualTo(new[] { "https://cdn.test/1.jpg", "https://cdn.test/2.jpg" }));
            Assert.That(uk.PriceFrom, Is.EqualTo(1300m));
            Assert.That(uk.Variants.Select(x => (x.Sku, x.FabricName, x.Prices!.Retail)), Is.EqualTo(new[]
            {
                ("MOD-001-A", "Грета", 1500m),
                ("MOD-001-B", "Саржа", 1700m),
            }));
            // A Russian request without Russian texts gets the Ukrainian ones and says so.
            Assert.That(ru.Language, Is.EqualTo("uk"));
            Assert.That(ru.Name, Is.EqualTo(uk.Name));
        });
    }

    [Test]
    public async Task Hidden_inactive_and_unknown_addresses_are_not_found()
    {
        using var h = Site();
        h.AddSewing();
        h.AddSewing(2, "Куртка Зима", "MOD-002", active: false);
        await ShowAsync(h, 1, "kostyum-lider", showOnSite: false);
        await ShowAsync(h, 2, "kurtka-zyma");
        var handler = new GetPublicCatalogProductHandler(h.Site(), h.Catalog);

        Assert.Multiple(async () =>
        {
            Assert.That((await handler.Handle(new("kostyum-lider", "uk"), None)).Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That((await handler.Handle(new("kurtka-zyma", "uk"), None)).Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That((await handler.Handle(new("nema", "uk"), None)).Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task Addresses_say_which_pages_have_russian_texts()
    {
        using var h = Site();
        h.AddSewing();
        h.AddSewing(2, "Куртка Зима", "MOD-002");
        await ShowAsync(h, 1, "kostyum-lider", russian: true);
        await ShowAsync(h, 2, "kurtka-zyma");

        var slugs = (await new GetPublicCatalogProductSlugsHandler(h.Site()).Handle(new(), None)).Value;

        Assert.That(slugs.Select(x => (x.Slug, x.HasRussian)),
            Is.EqualTo(new[] { ("kostyum-lider", true), ("kurtka-zyma", false) }));
    }

    [Test]
    public async Task Catalog_is_kept_for_a_short_time_so_a_change_reaches_the_site_after_it()
    {
        using var h = Site();
        h.AddSewing();
        await ShowAsync(h, 1, "kostyum-lider");
        using var cache = new MemoryCache(new MemoryCacheOptions());

        var before = await h.Site(cache).GetAsync(None);
        h.AddSewing(2, "Куртка Зима", "MOD-002");
        await ShowAsync(h, 2, "kurtka-zyma");
        var kept = await h.Site(cache).GetAsync(None);
        cache.Compact(1);
        var after = await h.Site(cache).GetAsync(None);

        Assert.Multiple(() =>
        {
            Assert.That(before.Products, Has.Count.EqualTo(1));
            Assert.That(kept, Is.SameAs(before));
            Assert.That(after.Products, Has.Count.EqualTo(2));
            Assert.That(PublicCatalogSource.Lifetime, Is.LessThanOrEqualTo(TimeSpan.FromMinutes(5)));
        });
    }

    [Test]
    public void Queries_refuse_a_wrong_language_type_and_page_size()
    {
        var list = new GetPublicCatalogProductsValidator();
        var one = new GetPublicCatalogProductValidator();
        Assert.Multiple(() =>
        {
            list.TestValidate(new GetPublicCatalogProductsQuery(1, 24, "uk", "Sewing", 3)).ShouldNotHaveAnyValidationErrors();
            list.TestValidate(new GetPublicCatalogProductsQuery(1, 24, "en")).ShouldHaveValidationErrorFor(x => x.Language);
            list.TestValidate(new GetPublicCatalogProductsQuery(1, 101, "uk")).ShouldHaveValidationErrorFor(x => x.PageSize);
            list.TestValidate(new GetPublicCatalogProductsQuery(1, 24, "uk", "Other")).ShouldHaveValidationErrorFor(x => x.Type);
            one.TestValidate(new GetPublicCatalogProductQuery("", "uk")).ShouldHaveValidationErrorFor(x => x.Slug);
            one.TestValidate(new GetPublicCatalogProductQuery("a", "de")).ShouldHaveValidationErrorFor(x => x.Language);
        });
    }

    [TestCase(typeof(PublicCatalogTree), "PublicCatalogTree")]
    [TestCase(typeof(PublicCatalogType), "PublicCatalogType")]
    [TestCase(typeof(PublicCatalogSection), "PublicCatalogSection")]
    [TestCase(typeof(PublicCatalogCard), "PublicCatalogCard")]
    [TestCase(typeof(PublicCatalogCardPage), "PublicCatalogCardPage")]
    [TestCase(typeof(PublicCatalogSectionLink), "PublicCatalogSectionLink")]
    [TestCase(typeof(PublicCatalogPhoto), "PublicCatalogPhoto")]
    [TestCase(typeof(PublicCatalogVariant), "PublicCatalogVariant")]
    [TestCase(typeof(PublicCatalogProduct), "PublicCatalogProduct")]
    [TestCase(typeof(PublicCatalogSlug), "PublicCatalogSlug")]
    [TestCase(typeof(PublicCatalogCategoryTree), "PublicCatalogCategoryTree")]
    [TestCase(typeof(PublicCatalogCategory), "PublicCatalogCategory")]
    [TestCase(typeof(PublicCatalogCategoryLink), "PublicCatalogCategoryLink")]
    [TestCase(typeof(PublicCatalogChildCategory), "PublicCatalogChildCategory")]
    [TestCase(typeof(PublicCatalogCategoryPage), "PublicCatalogCategoryPage")]
    [TestCase(typeof(PublicCatalogProductCategory), "PublicCatalogProductCategory")]
    public void Serialized_properties_match_the_machine_contract(Type type, string schema)
    {
        var properties = (YamlMappingNode)((YamlMappingNode)Schemas()[schema])["properties"];
        Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(x => x.Value),
            Is.EquivalentTo(type.GetProperties().Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))));
    }

    [Test]
    public void Every_operation_is_in_the_contract_open_to_everyone_and_read_only()
    {
        var controller = "PublicCatalog";
        var methods = MappedEndpoints.Of(Map, controller)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(methods.All(x => x.Anonymous), Is.True);
            Assert.That(methods.All(x => x.Method == "GET"), Is.True);
            Assert.That(methods.Select(x => x.Name), Is.EquivalentTo(Operations()));
            Assert.That(methods, Has.Length.EqualTo(7));
        });
    }

    [Test]
    public void Answers_carry_nothing_internal()
    {
        string[] forbidden = ["cost", "supplier", "problem", "updatedBy", "revision", "productId", "articleId", "isActive"];
        var names = new[]
            {
                typeof(PublicCatalogCard), typeof(PublicCatalogProduct), typeof(PublicCatalogVariant),
                typeof(PublicCatalogSection), typeof(PublicCatalogSlug), typeof(PublicCatalogCategory),
                typeof(PublicCatalogCategoryPage), typeof(PublicCatalogProductCategory),
            }
            .SelectMany(x => x.GetProperties()).Select(x => x.Name).ToArray();

        Assert.That(names.Where(name => forbidden.Any(x => name.Contains(x, StringComparison.OrdinalIgnoreCase))),
            Is.Empty);
    }

    private static YamlMappingNode Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        using var reader = File.OpenText(Path.Combine(directory!.FullName, "docs/sdd/contracts/content/content-public-catalog.openapi.yaml"));
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
