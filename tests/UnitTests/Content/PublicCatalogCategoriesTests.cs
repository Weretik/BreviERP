using Ardalis.Result;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using FluentValidation.TestHelper;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategorySlugs;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;
using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;
using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

namespace UnitTests.Content;

/// <summary>The public catalog by site category: tree, category page, listing, breadcrumbs, addresses (CNT-003 SC-06).</summary>
public sealed class PublicCatalogCategoriesTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static async Task<int> Category(ContentTestHarness h, string slug, string name, int? parentId = null,
        bool show = true, string? russian = null) =>
        (await h.SaveCategory().Handle(new SaveSiteCategoryCommand(null, 0, parentId, slug, show,
            new SiteCategoryTextInput(name, $"Купити {name}", $"Опис {name}",
                [new ProductTextBlock(ProductTextBlockKind.Paragraph, $"Текст {name}.", null, null)]),
            russian is null ? null : new SiteCategoryTextInput(russian, null, null, null),
            ContentTestHarness.Editor), None)).Value.Category.Id;

    private static async Task Show(ContentTestHarness h, int productId, string slug, params int[] categoryIds) =>
        Assert.That((await h.Save().Handle(new SaveContentProductCommand(productId, 0, slug, true,
            ContentTestHarness.Text(), null, ContentTestHarness.Editor, categoryIds), None)).IsSuccess, Is.True);

    /// <summary>
    /// «Спецодяг» (Russian name) with «Костюми», «Куртки» and a hidden «Халати»; an empty «Засоби захисту»; a hidden
    /// «Архів» with a marked subcategory. Product 2 is in «Спецодяг» itself and in «Куртки», product 1 in «Костюми»,
    /// product 3 (switched off in «Товари») in «Костюми».
    /// </summary>
    private static async Task<ContentTestHarness> Site()
    {
        var h = new ContentTestHarness();
        h.AddSewing();
        h.AddSewing(2, "Куртка Зима", "MOD-002");
        h.AddSewing(3, "Костюм Вимкнений", "MOD-003", active: false);
        var clothes = await Category(h, "spetsodyah", "Спецодяг", russian: "Спецодежда");
        var suits = await Category(h, "kostyumy", "Костюми", clothes);
        var jackets = await Category(h, "kurtky", "Куртки", clothes);
        var robes = await Category(h, "khalaty", "Халати", clothes, show: false);
        await Category(h, "zasoby-zakhystu", "Засоби захисту");
        var archive = await Category(h, "arkhiv", "Архів", show: false);
        await Category(h, "stari-modeli", "Старі моделі", archive);
        await Show(h, 2, "kurtka-zyma", clothes, jackets, robes);
        await Show(h, 1, "kostyum-lider", suits);
        await Show(h, 3, "kostyum-vymknenyy", suits);
        return h;
    }

    [Test]
    public async Task Tree_has_shown_categories_with_shown_parents_and_counts_products_once()
    {
        using var h = await Site();

        var tree = (await new GetPublicCatalogCategoriesHandler(h.Site()).Handle(new(), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(tree.Categories.Select(x => (x.Slug, x.NameUk, x.NameRu, x.ProductCount)), Is.EqualTo(new[]
            {
                ("spetsodyah", "Спецодяг", "Спецодежда", 2),
                ("zasoby-zakhystu", "Засоби захисту", "Засоби захисту", 0),
            }));
            Assert.That(tree.Categories[0].Children.Select(x => (x.Slug, x.ProductCount)), Is.EqualTo(new[]
            {
                ("kostyumy", 1),
                ("kurtky", 1),
            }));
        });
    }

    [Test]
    public async Task Category_page_has_seo_text_parent_and_subcategories_in_the_asked_language()
    {
        using var h = await Site();
        var handler = new GetPublicCatalogCategoryHandler(h.Site());

        var top = (await handler.Handle(new("spetsodyah", "ru"), None)).Value;
        var child = (await handler.Handle(new("kostyumy", "ru"), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That((top.Name, top.Language, top.Parent, top.ProductCount),
                Is.EqualTo(("Спецодежда", "ru", (object?)null, 2)));
            // The Russian text has only a name: SEO of that language is empty, nothing is mixed in.
            Assert.That((top.SeoTitle, top.Text.Count), Is.EqualTo(((string?)null, 0)));
            Assert.That(top.Children.Select(x => (x.Slug, x.Name, x.ProductCount)), Is.EqualTo(new[]
            {
                ("kostyumy", "Костюми", 1),
                ("kurtky", "Куртки", 1),
            }));
            Assert.That((child.Name, child.Language, child.SeoTitle, child.SeoDescription),
                Is.EqualTo(("Костюми", "uk", "Купити Костюми", "Опис Костюми")));
            Assert.That(child.Text.Single().Text, Is.EqualTo("Текст Костюми."));
            Assert.That(child.Parent, Is.EqualTo(new PublicCatalogCategoryLink("spetsodyah", "Спецодежда")));
        });
    }

    [Test]
    public async Task Hidden_categories_and_those_under_a_hidden_one_are_not_found()
    {
        using var h = await Site();
        var site = h.Site();
        var page = new GetPublicCatalogCategoryHandler(site);
        var cards = new GetPublicCatalogProductsHandler(site);

        Assert.Multiple(async () =>
        {
            foreach (var slug in new[] { "khalaty", "arkhiv", "stari-modeli", "nemaye" })
            {
                Assert.That((await page.Handle(new(slug, "uk"), None)).Status, Is.EqualTo(ResultStatus.NotFound), slug);
                Assert.That((await cards.Handle(new(1, 24, "uk", Category: slug), None)).Status,
                    Is.EqualTo(ResultStatus.NotFound), slug);
            }
        });
    }

    [Test]
    public async Task Listing_of_a_category_has_its_products_then_those_of_subcategories_each_once()
    {
        using var h = await Site();
        var cards = new GetPublicCatalogProductsHandler(h.Site());

        var top = (await cards.Handle(new(1, 24, "uk", Category: "spetsodyah"), None)).Value;
        var suits = (await cards.Handle(new(1, 24, "uk", Category: "kostyumy"), None)).Value;
        var empty = (await cards.Handle(new(1, 24, "uk", Category: "zasoby-zakhystu"), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(top.Items.Select(x => x.Slug), Is.EqualTo(new[] { "kurtka-zyma", "kostyum-lider" }));
            Assert.That(top.Total, Is.EqualTo(2));
            Assert.That(suits.Items.Select(x => x.Slug), Is.EqualTo(new[] { "kostyum-lider" }));
            Assert.That(empty.Total, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task Product_names_its_shown_categories_with_the_parent_for_breadcrumbs()
    {
        using var h = await Site();

        var product = (await new GetPublicCatalogProductHandler(h.Site(), h.Catalog)
            .Handle(new("kurtka-zyma", "ru"), None)).Value;

        Assert.That(product.Categories.Select(x => (x.Slug, x.Name, x.Parent?.Slug, x.Parent?.Name)), Is.EqualTo(new[]
        {
            ("spetsodyah", "Спецодежда", (string?)null, (string?)null),
            ("kurtky", "Куртки", "spetsodyah", "Спецодежда"),
        }));
    }

    [Test]
    public async Task Addresses_list_shown_categories_and_say_which_have_russian_texts()
    {
        using var h = await Site();

        var slugs = (await new GetPublicCatalogCategorySlugsHandler(h.Site()).Handle(new(), None)).Value;

        Assert.That(slugs.Select(x => (x.Slug, x.HasRussian)), Is.EqualTo(new[]
        {
            ("spetsodyah", true),
            ("kostyumy", false),
            ("kurtky", false),
            ("zasoby-zakhystu", false),
        }));
    }

    [Test]
    public void Queries_refuse_a_wrong_language_and_an_empty_address()
    {
        var page = new GetPublicCatalogCategoryValidator();
        var list = new GetPublicCatalogProductsValidator();
        Assert.Multiple(() =>
        {
            page.TestValidate(new GetPublicCatalogCategoryQuery("kostyumy", "uk")).ShouldNotHaveAnyValidationErrors();
            page.TestValidate(new GetPublicCatalogCategoryQuery("", "uk")).ShouldHaveValidationErrorFor(x => x.Slug);
            page.TestValidate(new GetPublicCatalogCategoryQuery("a", "de")).ShouldHaveValidationErrorFor(x => x.Language);
            list.TestValidate(new GetPublicCatalogProductsQuery(1, 24, "uk", Category: ""))
                .ShouldHaveValidationErrorFor(x => x.Category);
            list.TestValidate(new GetPublicCatalogProductsQuery(1, 24, "uk", Category: new string('a', 121)))
                .ShouldHaveValidationErrorFor(x => x.Category);
        });
    }
}
