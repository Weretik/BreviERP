using Crm.Application.Documents.Projection;
using Crm.Application.Quotes.CreateQuote;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;
using System.Text.Json;

namespace UnitTests.Crm.Documents;

public sealed class IntroductionQuotePinTests
{
    [Test]
    public void Introduction_requires_exact_saved_catalog_identity_and_price()
    {
        var slots = IntroductionSlot.Required(IntroductionLayout.Summer);
        var pin = new IntroductionPublicSnapshot(IntroductionLayout.Summer, Guid.NewGuid(),
            Guid.NewGuid(), slots.Select((slot, i) => new IntroductionPublicSlotSnapshot(slot,
                100 + i, 200 + i, $"Модель {i}", "Тканина", "Бавовна", "240 г/м²",
                $"https://brevi.com.ua/products/{i}", 700)).ToArray());
        var items = slots.Select((_, i) => Item(i, 100 + i, 200 + i, 700)).ToArray();
        Assert.That(IntroductionQuotePinValidator.Matches(pin, items), Is.True);
        Assert.That(IntroductionQuotePinValidator.Matches(pin, items.Select((x, i) =>
            i == 0 ? Item(i, 100, 200, 701) : x).ToArray()), Is.False);
        Assert.That(IntroductionQuotePinValidator.Matches(pin, items.Select((x, i) =>
            i == 0 ? Item(i, 999, 200, 700) : x).ToArray()), Is.False);
    }

    [Test]
    public void Projection_selector_uses_pinned_introduction_and_preserves_ordinary_quote_path()
    {
        var slots = IntroductionSlot.Required(IntroductionLayout.Summer);
        var pin = new IntroductionPublicSnapshot(IntroductionLayout.Summer, Guid.NewGuid(),
            Guid.NewGuid(), slots.Select((slot, i) => new IntroductionPublicSlotSnapshot(slot,
                100 + i, 200 + i, $"Модель {i}", "Тканина", "Бавовна", "240 г/м²",
                $"https://brevi.com.ua/products/{i}", 700)).ToArray());
        var item = Item(0, 100, 200, 700);
        var intro = QuoteVersion.Create(new QuoteId(1), 1, null, "created", "Клієнт",
            null, null, 7, [item], pin);
        var ordinary = QuoteVersion.Create(new QuoteId(2), 1, null, "created", "Клієнт",
            null, null, 7, [item]);
        var issuer = IssuerSettingsVersion.Create(1, "ПП Бреві", "35521636",
            "355216316015", "UA633052990000026004041206862", "Полтава",
            "+380951645855", "office@brevi.example", "https://brevi.example",
            null, null, null, new CrmUserId(7));
        var template = DocumentTemplateVersion.Create(1, "standard", "КП", "Без ПДВ",
            "Вітаємо", "Є питання?", "Зверніться", "Менеджер: {managerName}", new CrmUserId(7));

        var introDocument = QuoteDocumentProjectionSelector.Build(intro, issuer, template,
            "10/10/2026", "Олена");
        var ordinaryDocument = QuoteDocumentProjectionSelector.Build(ordinary, issuer, template,
            "10/10/2026", "Олена");

        Assert.Multiple(() =>
        {
            Assert.That(introDocument.Pages, Has.Count.EqualTo(1));
            Assert.That(introDocument.Pages![0].Products, Has.Count.EqualTo(4));
            Assert.That(ordinaryDocument.Pages, Is.Null);
            Assert.That(ordinaryDocument.Paragraphs, Is.Not.Empty);
        });
    }

    [Test]
    public void Json_round_trip_materializes_interface_typed_slot_list()
    {
        var slots = IntroductionSlot.Required(IntroductionLayout.General);
        var pin = new IntroductionPublicSnapshot(IntroductionLayout.General, Guid.NewGuid(),
            Guid.NewGuid(), slots.Select((slot, i) => new IntroductionPublicSlotSnapshot(slot,
                100 + i, 200 + i, $"Модель {i}", "Тканина", "Бавовна", "240 г/м²",
                $"https://brevi.com.ua/products/{i}", 700)).ToArray());
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(pin, options);
        var loaded = JsonSerializer.Deserialize<IntroductionPublicSnapshot>(json, options);

        Assert.Multiple(() =>
        {
            Assert.That(loaded?.Layout, Is.EqualTo(IntroductionLayout.General));
            Assert.That(loaded?.Slots, Has.Count.EqualTo(8));
            Assert.That(loaded?.Slots[4].Slot.Season, Is.EqualTo(IntroductionSeason.Winter));
            Assert.That(loaded?.Slots[4].LargeWholesale, Is.EqualTo(700));
        });
    }

    private static QuoteItem Item(int order, int productId, long articleId, decimal largePrice)
        => QuoteItem.Create(Guid.NewGuid(), order, $"Модель {order}", null, null, null,
            true, [QuoteVariant.Create(Guid.NewGuid(), 0, "Варіант", "sewing", productId,
                articleId, true, Guid.NewGuid(), "{}", [
                    QuoteTier.Create("retail", 900, false),
                    QuoteTier.Create("wholesale", 800, false),
                    QuoteTier.Create("largeWholesale", largePrice, true)])]);
}
