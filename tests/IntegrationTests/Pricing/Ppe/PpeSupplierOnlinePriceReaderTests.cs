using System.Net;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;
using Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online;

namespace IntegrationTests.Pricing.Ppe;

/// <summary>
/// АВ-Центр and Перчатка-центр pages and the 3C-Маркет lists read like the Apps Script, against a fake web (no network,
/// no database). CAT-017 stage 2.
/// </summary>
public sealed class PpeSupplierOnlinePriceReaderTests
{
    [TestCase("<script>var p = {\"price\": \"1 234,50\"};</script>", 1234.50)]
    [TestCase("<div class='x'>Ціна: <b>812</b> грн</div>", 812)]
    [TestCase("<span class=\"product-price\">\n  <i>₴</i> 455,00</span>", 455)]
    public void Av_center_page_price_follows_the_script_order(string html, decimal expected)
        => Assert.That(AvCenterPagePrice.Find(html), Is.EqualTo(expected));

    [TestCase("<div price-value=\"1 020.00\"></div>", 1020)]
    [TestCase("<meta property=\"product:price:amount\" content=\"77.5\">", 77.5)]
    [TestCase("<p>Цена</p><span>  1 299,90 грн</span>", 1299.90)]
    [TestCase("{\"price\": 64.2}", 64.2)]
    public void Perchatka_centr_page_price_follows_the_script_order(string html, decimal expected)
        => Assert.That(PerchatkaCentrPagePrice.Find(html), Is.EqualTo(expected));

    [Test]
    public void Pages_without_a_price_give_nothing()
        => Assert.Multiple(() =>
        {
            Assert.That(AvCenterPagePrice.Find("<p>Немає в наявності</p>"), Is.Null);
            Assert.That(PerchatkaCentrPagePrice.Find("<p>Немає в наявності</p>"), Is.Null);
        });

    [Test]
    public void Three_c_list_takes_the_last_number_and_digit_articles_without_leading_zeros()
    {
        var csv = "﻿Група,Назва,Артикул,Опт,Ціна\n" +
                  "Одяг,\"Костюм \"\"Захист\"\", синій\",0020,100,\"1 250,50\"\n" +
                  "Одяг,Без ціни,0021,,\n" +
                  "Одяг,Літерний,AB12,10,20\n";
        Assert.That(ThreeCMarketPriceLists.Parse(csv), Is.EqualTo(new Dictionary<string, decimal> { ["20"] = 1250.50m }));
    }

    [Test]
    public async Task Website_reads_each_link_once_and_reports_problems_per_page()
    {
        var web = new FakeWeb(new Dictionary<string, (HttpStatusCode, string)>
        {
            ["https://avcentr.com.ua/ua/a"] = (HttpStatusCode.OK, "Ціна: 100 грн"),
            ["https://avcentr.com.ua/ua/b"] = (HttpStatusCode.NotFound, ""),
            ["https://avcentr.com.ua/ua/c"] = (HttpStatusCode.OK, "<p>Немає</p>"),
        });
        var reader = new PpeSupplierOnlinePriceReader(new SupplierPageClient(new HttpClient(web)));

        var result = await reader.ReadAsync(PpePriceSources.Find("av-center")!,
        [
            Product("https://avcentr.com.ua/ua/a"), Product("https://www.avcentr.com.ua/ua/a/"),
            Product("https://avcentr.com.ua/ua/b"), Product("https://avcentr.com.ua/ua/c"),
            Product("https://example.com/x"), Product(null)
        ], CancellationToken.None);

        var byUrl = result.Value.ToDictionary(x => x.Url!);
        Assert.Multiple(() =>
        {
            Assert.That(web.Requests, Is.EquivalentTo(new[]
                { "https://avcentr.com.ua/ua/a", "https://avcentr.com.ua/ua/b", "https://avcentr.com.ua/ua/c" }));
            Assert.That(web.UserAgents.Distinct(), Is.EqualTo(new[] { "Mozilla/5.0 (compatible; BreviPriceUpdater/1.0)" }));
            Assert.That(byUrl["https://avcentr.com.ua/ua/a"].Price, Is.EqualTo(100m));
            Assert.That(byUrl["https://avcentr.com.ua/ua/b"].Problem, Does.Contain("404"));
            Assert.That(byUrl["https://avcentr.com.ua/ua/c"].Problem, Does.Contain("не знайдено ціну"));
            Assert.That(byUrl["https://example.com/x"].Problem, Does.Contain("avcentr.com.ua"));
        });
    }

    [Test]
    public async Task Three_c_reads_all_six_lists_and_fails_when_one_does_not_load()
    {
        var lists = ThreeCMarketPriceLists.All.ToDictionary(x => x.Url, x => (HttpStatusCode.OK, "a,b,10,50\n"));
        lists[ThreeCMarketPriceLists.All[1].Url] = (HttpStatusCode.OK, "a,b,10,60\na,b,11,70\n");
        var reader = new PpeSupplierOnlinePriceReader(new SupplierPageClient(new HttpClient(new FakeWeb(lists))));

        var merged = await reader.ReadAsync(PpePriceSources.Find("3c-market")!, [], CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(merged.Value.Single(x => x.Sku == "11").Price, Is.EqualTo(70m));
            Assert.That(merged.Value.Single(x => x.Sku == "10").Problem, Does.Contain("різними цінами"));
        });

        lists[ThreeCMarketPriceLists.All[5].Url] = (HttpStatusCode.InternalServerError, "");
        var failed = await reader.ReadAsync(PpePriceSources.Find("3c-market")!, [], CancellationToken.None);
        Assert.That(failed.ValidationErrors.Single().ErrorMessage, Does.Contain("Діелектрика").And.Contain("500"));
    }

    private static PpePriceImportProduct Product(string? url)
        => new(1, "1", "Товар", true, false, null, url, 10m, null, null);

    private sealed class FakeWeb(IDictionary<string, (HttpStatusCode Status, string Body)> pages) : HttpMessageHandler
    {
        private readonly object _lock = new();
        public List<string> Requests { get; } = [];
        public List<string> UserAgents { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.ToString();
            lock (_lock)
            {
                Requests.Add(url);
                UserAgents.Add(request.Headers.UserAgent.ToString());
            }

            var (status, body) = pages.TryGetValue(url, out var page) ? page : (HttpStatusCode.NotFound, "");
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }
}
