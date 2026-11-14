using Catalog.Application.Features.Product.Overview.Update;
using Catalog.Application.Features.Product.Overview.Update.Validators;
using Catalog.Domain.Products;

namespace UnitTests;

/// <summary>«Товари» page fields kept in the product catalog profile (CAT-006).</summary>
public sealed class ProductOverviewTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);

    [Test]
    public void Overview_fields_go_to_the_profile_trimmed()
    {
        var product = Create(ProductType.Sewing);

        product.SetOverview(Details(url: "  https://brevi.com.ua/specodyag-litnij/kostyum-verde ", quote: "   "), Now);

        var profile = product.CatalogProfile!;
        Assert.Multiple(() =>
        {
            Assert.That(profile.WebsiteUrl, Is.EqualTo("https://brevi.com.ua/specodyag-litnij/kostyum-verde"));
            Assert.That(profile.ShortDescription, Is.Null);
            Assert.That(profile.Season, Is.EqualTo("Літо"));
            Assert.That(profile.SectionId, Is.EqualTo(2));
            Assert.That(profile.IsActive, Is.True);
        });
    }

    [Test]
    public void Season_is_only_for_sewing_and_a_bad_link_is_rejected()
    {
        var ppe = Create(ProductType.Ppe);
        var sewing = Create(ProductType.Sewing);

        Assert.Multiple(() =>
        {
            Assert.That(ppe.SetOverview(Details(), Now).IsFailure, Is.True);
            Assert.That(sewing.SetOverview(Details(season: "Осінь"), Now).IsFailure, Is.True);
            Assert.That(sewing.SetOverview(Details(url: "brevi.com.ua/x"), Now).IsFailure, Is.True);
        });
    }

    [Test]
    public void Ppe_code_is_the_sale_sku_and_sewing_code_is_the_model_code()
    {
        var ppe = Create(ProductType.Ppe);
        ppe.SetCode(" 17 ", Now);
        Assert.That(ppe.CatalogProfile!.SaleSku, Is.EqualTo("17"));
        Assert.That(ppe.Code, Is.EqualTo("17"));
        Assert.That(ppe.SetCode(" ", Now).IsFailure, Is.True);

        var sewing = Create(ProductType.Sewing);
        sewing.SetCode("MOD-025", Now);
        sewing.SetCode("MOD-025", Now);
        Assert.That(sewing.Code, Is.EqualTo("MOD-025"));
        Assert.That(sewing.CompositionVersion, Is.EqualTo(1));
    }

    [TestCase("Осінь", null)]
    [TestCase(null, "ftp://brevi.com.ua/x")]
    public void Validator_rejects_unknown_season_and_non_web_link(string? season, string? url)
    {
        var result = new UpdateProductOverviewCommandValidator().Validate(
            new UpdateProductOverviewCommand(1, "MOD-001", 2, season, true, url, null));

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validator_accepts_a_sheet_row()
    {
        var result = new UpdateProductOverviewCommandValidator().Validate(new UpdateProductOverviewCommand(
            1, "MOD-025", 2, "Літо", true, "https://brevi.com.ua/specodyag-litnij/kostyum-verde",
            "Костюм із курткою та штанами."));

        Assert.That(result.IsValid, Is.True);
    }

    private static Product Create(ProductType type)
        => Product.Create(ProductId.Create(101), "Костюм «Верде»", "Костюм «Верде» RU", ProductSlug.Create("kostyum-verde").Value, type, Now).Value;

    private static ProductOverviewDetails Details(
        string? season = "Літо", string? url = null, string? quote = null)
        => new(2, season, true, url, quote);
}
