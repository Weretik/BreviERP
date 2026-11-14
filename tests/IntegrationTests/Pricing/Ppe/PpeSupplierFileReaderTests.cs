using System.IO.Compression;
using System.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;
using Catalog.Infrastructure.Integrations.PpeSupplierFiles;

namespace IntegrationTests.Pricing.Ppe;

/// <summary>Insight XML and СЕМТО PDF price files (CAT-017); no database needed.</summary>
public sealed class PpeSupplierFileReaderTests
{
    private readonly PpeSupplierFileReader reader = new();

    [Test]
    public async Task Insight_three_lists_give_dealer_wholesale_and_retail_per_link()
    {
        var result = await reader.ReadAsync(PpePriceFileKind.InsightYml,
        [
            Xml("a.xml", Offer("o445", "K10217/SH3", "1388.00"), Offer("o445", "K10217/MH3", "1388.00")),
            Xml("b.xml", Offer("o445", "K10217/SH3", "889.50"), Offer("o445", "K10217/MH3", "889.50"), Offer("o9", "X9", "10")),
            Gzip(Xml("c.xml", Offer("o445", "K10217/SH3", "1138.60"), Offer("o445", "K10217/MH3", "1138.60")))
        ], CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        var byLink = result.Value.Single(x => x.Url?.Contains("o445") == true);
        var missing = result.Value.Single(x => x.Url?.Contains("o9") == true);
        Assert.Multiple(() =>
        {
            Assert.That((byLink.Price, byLink.Wholesale, byLink.Retail), Is.EqualTo((889.5m, (decimal?)1138.6m, (decimal?)1388m)));
            Assert.That(byLink.Problem, Is.Null);
            Assert.That(missing.Problem, Does.Contain("не в усіх трьох"));
            Assert.That(result.Value.Any(x => x.Sku == "K10217/SH3" && x.Problem is null), Is.True);
        });
    }

    [Test]
    public async Task Insight_sizes_with_different_prices_in_one_list_are_a_problem()
    {
        var result = await reader.ReadAsync(PpePriceFileKind.InsightYml,
        [
            Xml("a.xml", Offer("o1", "A1", "100"), Offer("o1", "A2", "110")),
            Xml("b.xml", Offer("o1", "A1", "130")),
            Xml("c.xml", Offer("o1", "A1", "150"))
        ], CancellationToken.None);

        Assert.That(result.Value.Single(x => x.Url is not null).Problem, Does.Contain("різні ціни"));
    }

    [Test]
    public async Task Insight_rejects_a_file_that_is_not_xml()
    {
        var result = await reader.ReadAsync(PpePriceFileKind.InsightYml,
            [new SupplierPriceFile("price.pdf", new MemoryStream("%PDF-1.4"u8.ToArray())), Xml("b.xml"), Xml("c.xml")],
            CancellationToken.None);
        Assert.That(result.ValidationErrors.Single().ErrorMessage, Does.Contain("price.pdf"));
    }

    [Test]
    public async Task Cemto_pdf_rows_give_article_and_price_with_thousands_and_wrapped_articles()
    {
        var result = await reader.ReadAsync(PpePriceFileKind.CemtoPdf, [Pdf(page =>
        {
            Text(page, "5019", 67, 718); Text(page, "651,72", 244, 718);
            Text(page, "2023BLK", 56, 640); Text(page, "1", 240, 640); Text(page, "633,86", 247, 640);
            Text(page, "9054", 64, 564); Text(page, "winter", 61, 553); Text(page, "981,84", 244, 558);
            Text(page, "5020w", 62, 480); Text(page, "856,38", 257, 480); Text(page, "42,", 449, 480);
        })], CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ToDictionary(x => x.Sku!, x => x.Price), Is.EquivalentTo(new Dictionary<string, decimal>
        {
            ["5019"] = 651.72m, ["2023BLK"] = 1633.86m, ["9054 winter"] = 981.84m, ["5020w"] = 856.38m
        }));
    }

    [Test]
    public async Task Cemto_article_listed_with_two_prices_is_a_problem()
    {
        var result = await reader.ReadAsync(PpePriceFileKind.CemtoPdf, [Pdf(page =>
        {
            Text(page, "8011", 67, 700); Text(page, "620,22", 244, 700);
            Text(page, "8011", 67, 600); Text(page, "630,00", 244, 600);
        })], CancellationToken.None);

        Assert.That(result.Value.Single().Problem, Does.Contain("різні ціни"));
    }

    private static string Offer(string id, string sku, string price)
        => $"<offer id=\"{sku}\"><name>Жилет</name><url>https://insight.ua/ru/offer/item-{id}/</url><price>{price}</price><vendorCode>{sku}</vendorCode><description><![CDATA[<p>x</p>]]></description></offer>";

    private static SupplierPriceFile Xml(string name, params string[] offers)
        => new(name, new MemoryStream(Encoding.UTF8.GetBytes(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE yml_catalog SYSTEM \"shops.dtd\"><yml_catalog><shop><offers>"
            + string.Concat(offers) + "</offers></shop></yml_catalog>")));

    private static SupplierPriceFile Gzip(SupplierPriceFile file)
    {
        var packed = new MemoryStream();
        using (var gzip = new GZipStream(packed, CompressionMode.Compress, leaveOpen: true)) file.Content.CopyTo(gzip);
        packed.Position = 0;
        return file with { Content = packed };
    }

    private static PdfPageBuilder? currentPage;
    private static PdfDocumentBuilder.AddedFont? currentFont;

    private static SupplierPriceFile Pdf(Action<PdfPageBuilder> draw)
    {
        var builder = new PdfDocumentBuilder();
        currentFont = builder.AddStandard14Font(Standard14Font.Helvetica);
        currentPage = builder.AddPage(PageSize.A4);
        draw(currentPage);
        return new SupplierPriceFile("cemto.pdf", new MemoryStream(builder.Build()));
    }

    private static void Text(PdfPageBuilder page, string text, double x, double y)
        => page.AddText(text, 8, new PdfPoint(x, y), currentFont!);
}
