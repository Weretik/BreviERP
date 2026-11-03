using BuildingBlocks.Domain.Exceptions;
using Catalog.Application.Features.Product.Overview.Update;
using Catalog.Application.Features.Product.Overview.Update.Validators;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace UnitTests;

/// <summary>«Товари» page fields kept in the product catalog profile (CAT-006).</summary>
public sealed class ProductOverviewTests
{
    private static readonly DateOnly Today = new(2026, 11, 3);
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);

    [Test]
    public void Checked_product_keeps_its_date_or_gets_today_and_unchecked_loses_it()
    {
        var product = Create(ProductType.Sewing);

        product.SetOverview(Details(isVerified: true), Today, Now);
        Assert.That(product.CatalogProfile!.SourceReviewDate, Is.EqualTo("03.11.2026"));

        product.SetOverview(Details(isVerified: true), Today.AddDays(5), Now);
        Assert.That(product.CatalogProfile.SourceReviewDate, Is.EqualTo("03.11.2026"));

        product.SetOverview(Details(isVerified: false), Today, Now);
        Assert.That(product.CatalogProfile.IsSourceReviewed, Is.False);
        Assert.That(product.CatalogProfile.SourceReviewDate, Is.Null);
    }

    [Test]
    public void Overview_fields_go_to_the_profile_trimmed()
    {
        var product = Create(ProductType.Sewing);

        product.SetOverview(Details(url: "  https://brevi.com.ua/specodyag-litnij/kostyum-verde ", quote: "   "), Today, Now);

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
            Assert.Throws<DomainException>(() => ppe.SetOverview(Details(), Today, Now));
            Assert.Throws<DomainException>(() => sewing.SetOverview(Details(season: "Осінь"), Today, Now));
            Assert.Throws<DomainException>(() => sewing.SetOverview(Details(url: "brevi.com.ua/x"), Today, Now));
        });
    }

    [Test]
    public void Ppe_code_is_the_sale_sku_and_sewing_code_is_the_model_code()
    {
        var ppe = Create(ProductType.Ppe);
        ppe.SetCode(" 17 ", Now);
        Assert.That(ppe.CatalogProfile!.SaleSku, Is.EqualTo("17"));
        Assert.That(ppe.Code, Is.EqualTo("17"));
        Assert.Throws<DomainException>(() => ppe.SetCode(" ", Now));

        var sewing = Create(ProductType.Sewing);
        sewing.SetCode("MOD-025", Now);
        sewing.SetCode("MOD-025", Now);
        Assert.That(sewing.Code, Is.EqualTo("MOD-025"));
        Assert.That(sewing.CompositionVersion, Is.EqualTo(1));
    }

    [TestCase("Осінь", null, null)]
    [TestCase(null, "ftp://brevi.com.ua/x", null)]
    [TestCase(null, null, "2026-11-03")]
    public void Validator_rejects_unknown_season_non_web_link_and_bad_date(string? season, string? url, string? date)
    {
        var result = new UpdateProductOverviewCommandValidator().Validate(
            new UpdateProductOverviewCommand(1, "MOD-001", 2, season, true, true, date, url, null));

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validator_accepts_a_sheet_row()
    {
        var result = new UpdateProductOverviewCommandValidator().Validate(new UpdateProductOverviewCommand(
            1, "MOD-025", 2, "Літо", true, true, "08.09.2026", "https://brevi.com.ua/specodyag-litnij/kostyum-verde",
            "Костюм із курткою та штанами."));

        Assert.That(result.IsValid, Is.True);
    }

    private static Product Create(ProductType type)
        => Product.Create(ProductId.Create(101), "Костюм «Верде»", "Костюм «Верде» RU", ProductSlug.Create("kostyum-verde"), type, Now);

    private static ProductOverviewDetails Details(
        bool isVerified = false, string? season = "Літо", string? url = null, string? quote = null)
        => new(2, season, true, isVerified, null, url, quote);
}
