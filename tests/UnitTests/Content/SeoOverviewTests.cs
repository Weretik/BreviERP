using Content.Application.Contracts;
using Content.Application.ProductPages.Save;
using Content.Application.SeoOverview;
using Content.Application.SitePages.Save;
using Content.Domain.SitePages;

namespace UnitTests.Content;

/// <summary>«Контент → SEO-огляд»: only problems of what is on the site (CNT-008).</summary>
public sealed class SeoOverviewTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static async Task Show(ContentTestHarness h, int id, bool show, string? seoTitle, bool russian)
    {
        var result = await h.Save().Handle(new SaveContentProductCommand(id, 0, $"model-{id}", show,
            ContentTestHarness.Text($"Модель {id}", seoTitle),
            russian ? ContentTestHarness.Text($"Модель {id} ru", $"Купить модель {id}") : null,
            ContentTestHarness.Editor), None);
        Assert.That(result.IsSuccess, Is.True, string.Join(' ', result.Errors));
    }

    [Test]
    public async Task Lists_problems_of_pages_on_the_site_and_of_the_settings()
    {
        using var h = new ContentTestHarness();
        h.AddSewing(1, "Модель 1", "MOD-1");
        h.AddSewing(2, "Модель 2", "MOD-2");
        h.AddSewing(3, "Модель 3", "MOD-3");
        h.AddSewing(4, "Модель 4", "MOD-4");
        await Show(h, 1, true, "Купити костюм", russian: true);
        await Show(h, 2, true, null, russian: false);
        await Show(h, 3, true, "купити КОСТЮМ", russian: true);
        await Show(h, 4, false, null, russian: false);
        await h.SaveSitePage().Handle(new SaveSitePageCommand("wholesale", 0,
            new SitePageText("Спецодяг оптом", "Опис"), new SitePageText("Спецодежда оптом", "Описание"),
            ContentTestHarness.Editor), None);

        var overview = (await h.SeoOverview().Handle(new GetSeoOverviewQuery(), None)).Value;
        var products = overview.Issues.Where(x => x.Area == SeoArea.Product).ToDictionary(x => x.Id);
        var pages = overview.Areas.Single(x => x.Area == SeoArea.Page);

        Assert.Multiple(() =>
        {
            Assert.That(overview.SiteProblems, Is.EqualTo(Enum.GetValues<SeoSiteProblem>()));
            Assert.That(overview.Areas.Single(x => x.Area == SeoArea.Product),
                Is.EqualTo(new SeoAreaSummary(SeoArea.Product, 3, 3)));
            Assert.That(products["1"].Problems, Is.EqualTo(new[] { new SeoProblem(SeoProblemKind.DuplicateTitle, "uk") }));
            Assert.That(products["2"].Problems, Is.EqualTo(new[]
            {
                new SeoProblem(SeoProblemKind.NoTitle, "uk"), new SeoProblem(SeoProblemKind.NoRussian, null),
            }));
            Assert.That(products["2"].Path, Is.EqualTo("/product/model-2"));
            Assert.That(products.ContainsKey("4"), Is.False);
            Assert.That(pages.OnSite, Is.EqualTo(SitePageCatalog.Pages.Count - 1));
            Assert.That(overview.Issues.Any(x => x.Area == SeoArea.Page && x.Id == "wholesale"), Is.False);
            Assert.That(overview.Issues.Any(x => x.Id == "request"), Is.False);
        });
    }

    [Test]
    public async Task Long_texts_are_marked_as_cut_in_search()
    {
        using var h = new ContentTestHarness();
        await h.SaveSitePage().Handle(new SaveSitePageCommand("contacts", 0,
            new SitePageText(new string('а', 61), new string('б', 161)),
            new SitePageText("Контакты", "Описание"), ContentTestHarness.Editor), None);

        var overview = (await h.SeoOverview().Handle(new GetSeoOverviewQuery(), None)).Value;

        Assert.That(overview.Issues.Single(x => x.Id == "contacts").Problems, Is.EqualTo(new[]
        {
            new SeoProblem(SeoProblemKind.TitleTooLong, "uk"), new SeoProblem(SeoProblemKind.DescriptionTooLong, "uk"),
        }));
    }
}
