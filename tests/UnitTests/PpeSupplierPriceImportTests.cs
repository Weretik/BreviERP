using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Settings;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Settings.Validators;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Shared;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

namespace UnitTests;

/// <summary>«Імпорт цін»: supplier keys, the import plan and discount settings (CAT-017).</summary>
public sealed class PpeSupplierPriceImportTests
{
    private static readonly PpePriceSource Insight = PpePriceSources.Find("insight")!;
    private static readonly PpePriceSource Cemto = PpePriceSources.Find("cemto")!;

    [Test]
    public void Keys_match_the_script_normalization()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SupplierKeys.Sku(" k10217 /sh-3 "), Is.EqualTo(SupplierKeys.Sku("К10217/SН3")));
            Assert.That(SupplierKeys.Sku("2023 BLK"), Is.EqualTo("2023ВLК"));
            Assert.That(SupplierKeys.Sku("  "), Is.Null);
            Assert.That(SupplierKeys.Sku("0020"), Is.EqualTo("20"));
            Assert.That(SupplierKeys.Sku("000"), Is.EqualTo("0"));
            Assert.That(SupplierKeys.Url("https://insight.ua/ru/offer/jilet-o445/?utm=1"),
                Is.EqualTo(SupplierKeys.Url("http://www.insight.ua/offer/jilet-o445")));
            Assert.That(SupplierKeys.Name("3C-Макркет"), Is.EqualTo(SupplierKeys.Name("3С МАКРКЕТ")));
        });
    }

    [Test]
    public void Supplier_is_found_by_alias_or_by_the_saved_choice()
    {
        var suppliers = new Dictionary<int, Catalog.Application.Contracts.Reference.ReferenceItem>
        {
            [7] = new(7, "3C-Макркет"),
            [8] = new(8, "Інший")
        };
        var source = PpePriceSources.Find("3c-market")!;
        Assert.Multiple(() =>
        {
            Assert.That(PpePriceSourceSupplier.Resolve(source, null, suppliers)?.Id, Is.EqualTo(7));
            Assert.That(PpePriceSourceSupplier.Resolve(source,
                new PpePriceSourceSettings(source.Key, 8, 5, DateTimeOffset.UtcNow, "admin"), suppliers)?.Id, Is.EqualTo(8));
        });
    }

    [Test]
    public void Discount_comes_off_the_supplier_price_rounded_to_kopecks()
    {
        Assert.Multiple(() =>
        {
            Assert.That(PpePriceImportPlanner.ApplyDiscount(100m, 20m), Is.EqualTo(80m));
            Assert.That(PpePriceImportPlanner.ApplyDiscount(651.72m, 0m), Is.EqualTo(651.72m));
            Assert.That(PpePriceImportPlanner.ApplyDiscount(99.99m, 22m), Is.EqualTo(77.99m));
        });
    }

    [Test]
    public void Plan_skips_manual_and_inactive_products_and_reports_missing_ones()
    {
        var rows = PpePriceImportPlanner.Plan(Cemto, 0m,
            [
                Product(1, "1003", sku: "8011", price: 600m),
                Product(2, "1004", sku: "8010", price: 870.78m),
                Product(3, "1005", sku: "8012", manual: true),
                Product(4, "1006", sku: "8018", active: false),
                Product(5, "1007", sku: "9999"),
                Product(6, "1008")
            ],
            [File("8011", 620.22m), File("8010", 870.78m), File("8012", 1m), File("8018", 1m)], fromFiles: true);

        Assert.That(rows.Select(x => x.Status), Is.EqualTo(new[]
        {
            PpePriceImportStatus.Changed, PpePriceImportStatus.Unchanged, PpePriceImportStatus.ManualPrice,
            PpePriceImportStatus.Inactive, PpePriceImportStatus.NotInFile, PpePriceImportStatus.NotInFile
        }));
        Assert.Multiple(() =>
        {
            Assert.That(rows[0].NewPrice, Is.EqualTo(620.22m));
            Assert.That(rows[0].OldPrice, Is.EqualTo(600m));
            Assert.That(rows[5].Message, Does.Contain("немає посилання"));
        });
    }

    [Test]
    public void Insight_matches_by_link_and_takes_wholesale_and_retail_after_the_checks()
    {
        var product = Product(1, "2001", sku: "OLD", url: "https://insight.ua/offer/jilet-o445/", price: 800m);
        var link = "https://insight.ua/ru/offer/jilet-o445/";

        var changed = PpePriceImportPlanner.Plan(Insight, 0m, [product],
            [new SupplierFilePrice(null, link, null, 889.5m, 1138.6m, 1388m, null)], fromFiles: true).Single();
        var tooLow = PpePriceImportPlanner.Plan(Insight, 0m, [product],
            [new SupplierFilePrice(null, link, null, 889.5m, 900m, 1388m, null)], fromFiles: true).Single();
        var retailBelow = PpePriceImportPlanner.Plan(Insight, 0m, [product],
            [new SupplierFilePrice(null, link, null, 100m, 150m, 130m, null)], fromFiles: true).Single();
        var conflict = PpePriceImportPlanner.Plan(Insight, 0m, [product],
            [new SupplierFilePrice(null, link, null, 100m, null, null, "У розмірів в одному XML різні ціни.")], fromFiles: true).Single();

        Assert.Multiple(() =>
        {
            Assert.That(changed.Status, Is.EqualTo(PpePriceImportStatus.Changed));
            Assert.That((changed.NewPrice, changed.NewWholesale, changed.NewRetail), Is.EqualTo((889.5m, (decimal?)1138.6m, (decimal?)1388m)));
            Assert.That(tooLow.Message, Does.Contain("+20%"));
            Assert.That(retailBelow.Message, Does.Contain("нижча за оптову"));
            Assert.That(conflict.Status, Is.EqualTo(PpePriceImportStatus.Invalid));
        });
    }

    [Test]
    public void Website_and_3c_sources_apply_their_discount_and_explain_what_is_missing()
    {
        var avCenter = PpePriceSources.Find("av-center")!;
        var threeC = PpePriceSources.Find("3c-market")!;
        var page = "https://avcentr.com.ua/ua/item-1";

        var site = PpePriceImportPlanner.Plan(avCenter, 20m,
            [Product(1, "1", url: page, price: 50m), Product(2, "2"), Product(3, "3", url: "https://avcentr.com.ua/ua/x")],
            [new SupplierFilePrice(null, page, null, 100m, null, null, null),
             new SupplierFilePrice(null, "https://avcentr.com.ua/ua/x", null, 0m, null, null, "На сторінці постачальника не знайдено ціну.")], fromFiles: false);
        var lists = PpePriceImportPlanner.Plan(threeC, 5m, [Product(1, "1", sku: "0020"), Product(2, "2", sku: "21"), Product(3, "3")],
            [File("20", 200m)], fromFiles: false);

        Assert.Multiple(() =>
        {
            Assert.That(site[0].NewPrice, Is.EqualTo(80m));
            Assert.That(site[1].Message, Does.Contain("немає посилання на сторінку"));
            Assert.That((site[2].Status, site[2].Message), Is.EqualTo((PpePriceImportStatus.Invalid, "На сторінці постачальника не знайдено ціну.")));
            Assert.That(lists[0].NewPrice, Is.EqualTo(190m));
            Assert.That(lists[1].Message, Is.EqualTo("Артикул 21 не знайдено в прайсах 3C-Маркет."));
            Assert.That(lists[2].Message, Does.Contain("немає артикулу"));
        });
    }

    [Test]
    public void Trele_cabinet_price_changes_only_the_dealer_price()
    {
        var link = "https://trele.com.ua/p/1/";
        var product = new PpePriceImportProduct(1, "2001", "Жилет", true, false, null, link, 800m, 1100m, 1350m);
        var row = PpePriceImportPlanner.Plan(PpePriceSources.Find("trele")!, 0m, [product],
            [new SupplierFilePrice(null, link, null, 889.5m, null, null, null)], fromFiles: false).Single();
        Assert.That((row.Status, row.NewPrice, row.NewWholesale, row.NewRetail),
            Is.EqualTo((PpePriceImportStatus.Changed, (decimal?)889.5m, (decimal?)1100m, (decimal?)1350m)));
    }

    [Test]
    public void All_sources_except_files_can_be_refreshed()
        => Assert.That(PpePriceSources.All.Where(x => x.CanRefresh).Select(x => x.Key),
            Is.EqualTo(new[] { "av-center", "3c-market", "perchatka-centr", "trele" }));

    [TestCase(0, true)]
    [TestCase(22.5, true)]
    [TestCase(90, true)]
    [TestCase(-1, false)]
    [TestCase(91, false)]
    [TestCase(5.555, false)]
    public void Discount_is_between_0_and_90_with_two_decimals(decimal discount, bool valid)
        => Assert.That(new UpdatePpePriceSourceCommandValidator()
            .Validate(new UpdatePpePriceSourceCommand("av-center", discount, null, "admin")).IsValid, Is.EqualTo(valid));

    private static PpePriceImportProduct Product(int id, string code, string? sku = null, string? url = null,
        decimal? price = null, bool manual = false, bool active = true)
        => new(id, code, "Товар " + code, active, manual, sku, url, price, null, null);

    private static SupplierFilePrice File(string sku, decimal price) => new(sku, null, null, price, null, null, null);
}
