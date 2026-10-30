using Crm.Application.Documents.Projection;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Introductions;

namespace UnitTests.Crm.Documents;

public sealed class IntroductionPublicDocumentProjectionBuilderTests
{
    private static readonly IssuerSettingsVersion Issuer = IssuerSettingsVersion.Create(1,
        "ПП Бреві", "35521636", "355216316015", "UA633052990000026004041206862",
        "Полтава", "+380951645855", "office@brevi.example", "https://brevi.example",
        null, null, null, new CrmUserId(7));

    private static readonly DocumentTemplateVersion Template = DocumentTemplateVersion.Create(1,
        "standard", "Комерційна пропозиція", "Усі ціни вказані в грн з ПДВ.",
        "Вітаємо", "Є питання?", "Зверніться до нас", "Менеджер: {managerName}",
        new CrmUserId(7));

    [TestCase(IntroductionLayout.General, 2)]
    [TestCase(IntroductionLayout.Summer, 1)]
    [TestCase(IntroductionLayout.Winter, 1)]
    public void Public_projection_has_four_cards_per_requested_season_and_no_private_pricing_data(
        IntroductionLayout layout, int expectedPages)
    {
        var draft = Prepared(layout);
        var frozen = IntroductionPublicDocumentProjectionBuilder.Freeze(draft);
        var document = IntroductionPublicDocumentProjectionBuilder.Build(frozen, Issuer, Template,
            "10/10/2026", "Клієнт", "Олена");
        var publicText = string.Join("\n", document.Pages!.SelectMany(x =>
            x.Products.SelectMany(card => new[] { card.ModelName, card.FabricName,
                card.FabricComposition, card.FabricDensity, card.ProductUrl,
                card.LargeWholesale.ToString() })));

        Assert.Multiple(() =>
        {
            Assert.That(document.Title, Does.Contain("10/10/2026"));
            Assert.That(document.Pages, Has.Count.EqualTo(expectedPages));
            Assert.That(document.Pages!.All(x => x.Products.Count == 4), Is.True);
            Assert.That(publicText, Does.Contain("бавовна"));
            Assert.That(publicText, Does.Contain("https://brevi.com.ua/products/"));
            Assert.That(publicText, Does.Not.Contain("PRIVATE_FORMULA_SENTINEL"));
            Assert.That(publicText, Does.Not.Contain("PRIVATE_SOURCE_SENTINEL"));
            Assert.That(document.Paragraphs, Is.Empty);
            Assert.That(document.Images, Is.Empty);
        });
    }

    [Test]
    public void Public_projection_rejects_untrusted_url_and_missing_slot()
    {
        var original = IntroductionPublicDocumentProjectionBuilder.Freeze(Prepared(IntroductionLayout.Summer));
        var malicious = original with { Slots = original.Slots.Select((slot, index) =>
            index == 0 ? slot with { ProductUrl = "https://brevi.com.ua.evil.invalid/x" } : slot).ToArray() };
        Assert.That(() => IntroductionPublicDocumentProjectionBuilder.Build(malicious, Issuer,
            Template, "10/10/2026", "Клієнт", "Олена"),
            Throws.InvalidOperationException.With.Message.EqualTo("introduction_public_snapshot_invalid"));

        var missing = original with { Slots = original.Slots.Skip(1).ToArray() };
        Assert.That(() => IntroductionPublicDocumentProjectionBuilder.Build(missing, Issuer,
            Template, "10/10/2026", "Клієнт", "Олена"),
            Throws.InvalidOperationException.With.Message.EqualTo("introduction_public_snapshot_invalid"));
    }

    [Test]
    public void Oversized_template_paragraph_is_rejected_before_ready_instead_of_in_renderer()
    {
        var longTemplate = DocumentTemplateVersion.Create(2, "standard", "КП",
            "Усі ціни в грн", new string('а', 1001), "Контакт", "Напишіть нам",
            "{managerName}", new CrmUserId(7));
        var pin = IntroductionPublicDocumentProjectionBuilder.Freeze(Prepared(IntroductionLayout.Summer));
        Assert.That(() => IntroductionPublicDocumentProjectionBuilder.Build(pin, Issuer,
            longTemplate, "10/10/2026", "Клієнт", "Олена"),
            Throws.InvalidOperationException.With.Message.EqualTo("introduction_public_layout_too_large"));
    }

    [Test]
    public void Combined_legal_template_text_and_line_breaks_block_fixed_seasonal_layout()
    {
        var pin = IntroductionPublicDocumentProjectionBuilder.Freeze(Prepared(IntroductionLayout.General));
        foreach (var opening in new[] { new string('а', 600), string.Join("\n", Enumerable.Repeat("Рядок", 20)) })
        {
            var template = DocumentTemplateVersion.Create(2, "standard", "КП", "Ціни в грн",
                opening, "Контакт", new string('б', 600), "{managerName}", new CrmUserId(7));
            Assert.That(() => IntroductionPublicDocumentProjectionBuilder.Build(pin, Issuer,
                template, "10/10/2026", "Клієнт", "Олена"),
                Throws.InvalidOperationException.With.Message.EqualTo("introduction_public_layout_too_large"));
        }
    }

    private static IntroductionPreparedDraft Prepared(IntroductionLayout layout)
    {
        var slots = IntroductionSlot.Required(layout).Select((slot, index) =>
        {
            var productId = 100 + index;
            var articleId = 200L + index;
            var price = new QuoteCatalogPrice("sewing", productId, articleId, Guid.NewGuid(),
                null, null, null, null, [], "PRIVATE_FORMULA_SENTINEL", 900, 800, 700,
                ["PRIVATE_SOURCE_SENTINEL"]);
            return new IntroductionPreparedSlot(new IntroductionSelection(slot, productId, articleId),
                $"Модель {index} <script>private()</script>", "Тканина", "100% бавовна", "240 г/м²",
                $"https://brevi.com.ua/products/{productId}", 700, price);
        }).ToArray();
        return new IntroductionPreparedDraft(layout, Guid.NewGuid(), Guid.NewGuid(), slots);
    }
}
