using Crm.Application.Abstractions.Documents;
using Crm.Application.Documents.Projection;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;

namespace UnitTests.Crm.Documents;

public sealed class QuotePublicDocumentProjectionBuilderTests
{
    private static IssuerSettingsVersion Issuer => IssuerSettingsVersion.Create(1, "ПП Бреві", "35521636",
        "355216316015", "UA633052990000026004041206862", "Полтава", "+380951645855",
        "office@brevi.example", "https://brevi.example", null, null, null, new CrmUserId(7));

    private static DocumentTemplateVersion Template => DocumentTemplateVersion.Create(1, "standard",
        "Комерційна пропозиція", "Усі ціни вказані в грн з ПДВ.", "Вітаємо",
        "Є питання?", "Зверніться до нас", "Менеджер: {managerName}", new CrmUserId(7));

    [Test]
    public void Projection_includes_only_public_selection_and_visible_priced_tiers()
    {
        var visible = QuoteVariant.Create(Guid.NewGuid(), 1, "Публічний варіант", "oneOffPpe", null, null,
            true, Guid.NewGuid(), "PRIVATE_BASIS_SENTINEL", [
                QuoteTier.Create("largeWholesale", 70, false),
                QuoteTier.Create("wholesale", 80, true),
                QuoteTier.Create("retail", 90, true)]);
        var hidden = QuoteVariant.Create(Guid.NewGuid(), 0, "HIDDEN_VARIANT_SENTINEL", "oneOffPpe", null, null,
            false, Guid.NewGuid(), "PRIVATE_HIDDEN_BASIS", [
                QuoteTier.Create("retail", 900, true),
                QuoteTier.Create("wholesale", 800, true),
                QuoteTier.Create("largeWholesale", 700, true)]);
        var version = QuoteVersion.Create(new QuoteId(1), 1, null, "created", "Клієнт", null, null, 7, [
            QuoteItem.Create(Guid.NewGuid(), 0, "Публічна позиція", "Опис", "Коментар",
                "INTERNAL_NOTE_SENTINEL", true, [hidden, visible]),
            QuoteItem.Create(Guid.NewGuid(), 1, "HIDDEN_ITEM_SENTINEL", "HIDDEN_DESCRIPTION", null,
                "HIDDEN_INTERNAL", false, [visible])]);

        var document = QuotePublicDocumentProjectionBuilder.Build(version, Issuer, Template,
            "10/10/2026", "Олена <script>alert(1)</script>");
        var text = string.Join("\n", document.Paragraphs);

        Assert.Multiple(() =>
        {
            Assert.That(document.Title, Does.Contain("10/10/2026"));
            Assert.That(text, Does.Contain("Публічна позиція"));
            Assert.That(text, Does.Contain("Публічний варіант"));
            Assert.That(text, Does.Contain("90.00 грн"));
            Assert.That(text, Does.Contain("80.00 грн"));
            Assert.That(text, Does.Not.Contain("70.00 грн"));
            Assert.That(text, Does.Contain("Олена <script>alert(1)</script>"));
            Assert.That(text, Does.Not.Contain("INTERNAL_NOTE_SENTINEL"));
            Assert.That(text, Does.Not.Contain("PRIVATE_BASIS_SENTINEL"));
            Assert.That(text, Does.Not.Contain("HIDDEN_VARIANT_SENTINEL"));
            Assert.That(text, Does.Not.Contain("HIDDEN_ITEM_SENTINEL"));
            Assert.That(document.TextGroups!.SelectMany(group => group.Paragraphs), Is.EqualTo(document.Paragraphs));
            Assert.That(document.Images, Is.Empty);
            Assert.That(document.HeaderLogo, Is.Null);
        });
    }

    [Test]
    public void Projection_rejects_missing_visible_price_instead_of_printing_zero()
    {
        var variant = QuoteVariant.Create(Guid.NewGuid(), 0, "Варіант", "oneOffPpe", null, null,
            true, Guid.NewGuid(), "basis", [
                QuoteTier.Create("retail", null, true),
                QuoteTier.Create("wholesale", 80, false),
                QuoteTier.Create("largeWholesale", 70, false)]);
        var version = QuoteVersion.Create(new QuoteId(1), 1, null, "created", "Клієнт", null, null, 7,
            [QuoteItem.Create(Guid.NewGuid(), 0, "Позиція", null, null, null, true, [variant])]);

        Assert.That(() => QuotePublicDocumentProjectionBuilder.Build(version, Issuer, Template,
            "10/10/2026", "Олена"), Throws.InvalidOperationException.With.Message.EqualTo("public_tier_price_missing"));
    }

    [Test]
    public void Projection_does_not_silently_omit_a_selected_logo()
    {
        var issuerWithLogo = IssuerSettingsVersion.Create(1, "ПП Бреві", "35521636",
            "355216316015", "UA633052990000026004041206862", "Полтава", "+380951645855",
            "office@brevi.example", "https://brevi.example", null, null, "brevi-wordmark", new CrmUserId(7));
        var variant = QuoteVariant.Create(Guid.NewGuid(), 0, "Варіант", "oneOffPpe", null, null,
            true, Guid.NewGuid(), "basis", [
                QuoteTier.Create("retail", 90, true),
                QuoteTier.Create("wholesale", 80, false),
                QuoteTier.Create("largeWholesale", 70, false)]);
        var version = QuoteVersion.Create(new QuoteId(1), 1, null, "created", "Клієнт", null, null, 7,
            [QuoteItem.Create(Guid.NewGuid(), 0, "Позиція", null, null, null, true, [variant])]);

        Assert.That(() => QuotePublicDocumentProjectionBuilder.Build(version, issuerWithLogo, Template,
            "10/10/2026", "Олена"),
            Throws.InvalidOperationException.With.Message.EqualTo("public_logo_asset_missing_or_unexpected"));

        var logo = new PublicDocumentImage("Brevi", "image/png", Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/g9kAAAAASUVORK5CYII="));
        var document = QuotePublicDocumentProjectionBuilder.Build(version, issuerWithLogo, Template,
            "10/10/2026", "Олена", logo);
        Assert.Multiple(() =>
        {
            Assert.That(document.HeaderLogo, Is.SameAs(logo));
            Assert.That(document.TextGroups!.SelectMany(group => group.Paragraphs), Is.EqualTo(document.Paragraphs));
            Assert.That(document.Images, Is.Empty);
        });
    }
}
