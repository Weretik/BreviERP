using Ardalis.Result;
using System.Net;
using Microsoft.Extensions.Options;
using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;
using Catalog.Infrastructure.Integrations.PpeSupplierSites;
using Catalog.Infrastructure.Integrations.PpeSupplierSites.Cabinets;
using Catalog.Infrastructure.Integrations.PpeSupplierSites.Pages;

namespace IntegrationTests.Catalog.Pricing.Ppe.SupplierPrices;

/// <summary>
/// АВ-Центр and Перчатка-центр pages, the 3C-Маркет lists and the Трєле cabinet read like the Apps Script,
/// against a fake web (no network, no database). CAT-017 stages 2–3.
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
        var reader = Reader(web);

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
        var reader = Reader(new FakeWeb(lists));

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

    [Test]
    public void Cabinet_pages_give_the_dealer_price()
        => Assert.Multiple(() =>
        {
            Assert.That(new TreleCabinet().FindPrice("<span class=\"add2cart__price-default js-p\" data-price=\"412.8\">"), Is.EqualTo(412.8m));
            Assert.That(new TreleCabinet().FindPrice("<span class=\"price\">500 грн</span>"), Is.Null);
        });

    [Test]
    public async Task Trele_signs_in_once_and_reads_pages_with_the_session_cookie()
    {
        var web = new FakeWeb(new Dictionary<string, (HttpStatusCode, string)>
        {
            ["https://trele.com.ua/login/"] = (HttpStatusCode.OK, ""),
            ["https://trele.com.ua/p/1/"] = (HttpStatusCode.OK, "<span class=\"add2cart__price-default\" data-price=\"500\"></span>"),
        }, setCookie: "PHPSESSID=s1; path=/");
        var reader = Reader(web, new SupplierCabinetOptions { Trele = { Login = "dealer", Password = "secret" } });

        var result = await reader.ReadAsync(PpePriceSources.Find("trele")!,
            [Product("https://trele.com.ua/p/1/"), Product("https://trele.com.ua/p/1/")], CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Single().Price, Is.EqualTo(500m));
            Assert.That(web.Requests.Count(x => x == "https://trele.com.ua/login/"), Is.EqualTo(2));
            Assert.That(web.Cookies.Last(), Does.Contain("PHPSESSID=s1"));
        });
    }

    [Test]
    public async Task Cabinet_without_credentials_or_with_a_refused_login_changes_nothing()
    {
        var web = new FakeWeb(new Dictionary<string, (HttpStatusCode, string)>
        {
            ["https://trele.com.ua/login/"] = (HttpStatusCode.OK, ""),
        });
        var noLogin = await Reader(web).ReadAsync(PpePriceSources.Find("trele")!, [Product("https://trele.com.ua/p/1")], CancellationToken.None);
        var refused = await Reader(web, new SupplierCabinetOptions { Trele = { Login = "dealer", Password = "wrong" } })
            .ReadAsync(PpePriceSources.Find("trele")!, [Product("https://trele.com.ua/p/1")], CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(noLogin.ValidationErrors.Single().ErrorMessage, Does.Contain("ще не додано"));
            Assert.That(refused.ValidationErrors.Single().ErrorMessage, Does.Contain("Не вдалося увійти в кабінет Трєле"));
            Assert.That(web.Requests, Has.None.EqualTo("https://trele.com.ua/p/1"));
        });
    }

    [Test]
    public async Task Insight_is_not_read_online()
    {
        var web = new FakeWeb(new Dictionary<string, (HttpStatusCode, string)>());
        var result = await Reader(web).ReadAsync(PpePriceSources.Find("insight")!, [Product("https://insight.ua/offer/a/")], CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(web.Requests, Is.Empty);
        });
    }

    private static PpeSupplierOnlinePriceReader Reader(FakeWeb web, SupplierCabinetOptions? cabinets = null)
        => new(new SupplierPageClient(new HttpClient(web)), new SupplierCabinetPriceReader(
            new SupplierCabinetClient(new HttpClient(web)), Options.Create(cabinets ?? new SupplierCabinetOptions())));

    private static PpePriceImportProduct Product(string? url)
        => new(1, "1", "Товар", true, false, null, url, 10m, null, null);

    private sealed class FakeWeb(IDictionary<string, (HttpStatusCode Status, string Body)> pages, string? setCookie = null)
        : HttpMessageHandler
    {
        private readonly object _lock = new();
        public List<string> Requests { get; } = [];
        public List<string> UserAgents { get; } = [];
        public List<string> Cookies { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.ToString();
            lock (_lock)
            {
                Requests.Add(url);
                UserAgents.Add(request.Headers.UserAgent.ToString());
                Cookies.Add(request.Headers.TryGetValues("Cookie", out var cookie) ? string.Join("; ", cookie) : "");
            }

            var (status, body) = pages.TryGetValue(url, out var page) ? page : (HttpStatusCode.NotFound, "");
            var response = new HttpResponseMessage(status) { Content = new StringContent(body) };
            if (setCookie is not null) response.Headers.TryAddWithoutValidation("Set-Cookie", setCookie);
            return Task.FromResult(response);
        }
    }
}
