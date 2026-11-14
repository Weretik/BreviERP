using Content.Domain.ProductPages;

namespace UnitTests.Content;

/// <summary>Slug and text rules of a product page (CNT-002 R-03, R-04).</summary>
public sealed class ProductPageRulesTests
{
    [TestCase("kostyum-lider", true)]
    [TestCase("abc", true)]
    [TestCase("ab", false)]
    [TestCase("Kostyum", false)]
    [TestCase("-kostyum", false)]
    [TestCase("kostyum-", false)]
    [TestCase("kostyum--lider", false)]
    [TestCase("костюм", false)]
    [TestCase("kostyum lider", false)]
    [TestCase(null, false)]
    public void Slug_is_lower_latin_digits_and_single_hyphens(string? slug, bool valid) =>
        Assert.That(ProductPageRules.IsValidSlug(slug), Is.EqualTo(valid));

    [TestCase("Костюм «Лідер» (літній)", "kostyum-lider-litniy")]
    [TestCase("  Жилет  Safety 2  ", "zhylet-safety-2")]
    [TestCase("Об'єкт", "obyekt")]
    [TestCase("«»", "")]
    public void Slug_is_built_from_the_name(string name, string slug) =>
        Assert.That(ProductPageRules.SlugFrom(name), Is.EqualTo(slug));

    [Test]
    public void Long_name_gives_a_slug_within_the_limit()
    {
        var slug = ProductPageRules.SlugFrom(string.Join(' ', Enumerable.Repeat("костюм", 40)));
        Assert.That(ProductPageRules.IsValidSlug(slug), Is.True);
    }

    [Test]
    public void New_page_is_hidden_and_has_revision_zero()
    {
        var page = ProductPage.Create(7, "kostyum-lider", "a@b.c", DateTimeOffset.UnixEpoch).Value;
        Assert.Multiple(() =>
        {
            Assert.That(page.ShowOnSite, Is.False);
            Assert.That(page.Revision, Is.Zero);
            Assert.That(page.Texts, Is.Empty);
        });
    }

    [Test]
    public void Replace_cleans_texts_drops_empty_rows_and_empty_languages()
    {
        var page = ProductPage.Create(7, "kostyum-lider", "a@b.c", DateTimeOffset.UnixEpoch).Value;
        page.Replace("kostyum-lider", true, new Dictionary<string, ProductPageTextInput>
        {
            ["uk"] = ContentTestHarness.Text(),
            ["ru"] = new ProductPageTextInput(" ", null, null, [], null, null),
        }, "a@b.c", DateTimeOffset.UnixEpoch);

        var uk = page.TextIn("uk")!;
        Assert.Multiple(() =>
        {
            Assert.That(page.Revision, Is.EqualTo(1));
            Assert.That(page.TextIn("ru"), Is.Null);
            Assert.That(uk.Summary, Has.Count.EqualTo(1));
            Assert.That(uk.Description.Select(x => x.Kind), Is.EqualTo(new[]
            {
                ProductTextBlockKind.Heading, ProductTextBlockKind.Paragraph, ProductTextBlockKind.List,
            }));
            Assert.That(uk.Description[1].Text, Is.EqualTo("Зручний костюм."));
            Assert.That(uk.Description[1].Lead, Is.EqualTo("Куртка:"));
            Assert.That(uk.Description[2].Items, Is.EqualTo(new[] { "дві кишені" }));
            Assert.That(uk.Specs.Single().Rows, Has.Count.EqualTo(1));
            Assert.That(uk.HasText && uk.HasSeo, Is.True);
        });
    }

    [Test]
    public void Same_visibility_does_not_change_the_revision()
    {
        var page = ProductPage.Create(7, "kostyum-lider", "a@b.c", DateTimeOffset.UnixEpoch).Value;
        page.SetVisibility(false, "a@b.c", DateTimeOffset.UnixEpoch);
        Assert.That(page.Revision, Is.Zero);
        page.SetVisibility(true, "a@b.c", DateTimeOffset.UnixEpoch);
        Assert.That(page.Revision, Is.EqualTo(1));
    }
}
