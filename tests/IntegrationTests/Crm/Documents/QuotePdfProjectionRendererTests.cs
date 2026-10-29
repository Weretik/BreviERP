using Crm.Application.Documents.Projection;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Documents.Rendering;

namespace IntegrationTests.Crm.Documents;

public sealed class QuotePdfProjectionRendererTests
{
    [Test]
    public async Task Saved_quote_public_projection_renders_multipage_pdf_without_hidden_fields()
    {
        var executable = Environment.GetEnvironmentVariable("BREVIERP_CHROMIUM_TEST_EXECUTABLE");
        var font = Environment.GetEnvironmentVariable("BREVIERP_CHROMIUM_TEST_FONT");
        if (string.IsNullOrWhiteSpace(executable) || string.IsNullOrWhiteSpace(font))
            Assert.Ignore("Set the provisioned Chromium executable and font for this PDF proof.");

        var issuer = IssuerSettingsVersion.Create(1, "ПП Бреві", "35521636", "355216316015",
            "UA633052990000026004041206862", "Полтава", "+380951645855", "office@brevi.example",
            "https://brevi.example", null, null, null, new CrmUserId(7));
        var template = DocumentTemplateVersion.Create(1, "standard", "Комерційна пропозиція",
            "Усі ціни вказані в грн з ПДВ.", "Вітаємо!", "Є питання?", "Зверніться до нас",
            "Менеджер: {managerName}", new CrmUserId(7));
        var items = Enumerable.Range(0, 72).Select(index => QuoteItem.Create(Guid.NewGuid(), index,
            $"Позиція {index + 1}: Український текст", index == 0
                ? "<script>fetch('https://example.invalid/private')</script>"
                : "Український опис товару з деталями фасону, кольору та застосування.",
            "Доступний клієнту коментар.", "INTERNAL_NOTE_SENTINEL", true,
            [QuoteVariant.Create(Guid.NewGuid(), 0, $"Варіант {index + 1}", "oneOffPpe", null, null,
                true, Guid.NewGuid(), "PRIVATE_BASIS_SENTINEL", [
                    QuoteTier.Create("retail", 123.45m, true),
                    QuoteTier.Create("wholesale", 100m, false),
                    QuoteTier.Create("largeWholesale", 90m, false)])])).ToList();
        items.Add(QuoteItem.Create(Guid.NewGuid(), 72, "HIDDEN_ITEM_SENTINEL", "HIDDEN_DESCRIPTION_SENTINEL",
            null, "HIDDEN_INTERNAL_SENTINEL", false,
            [QuoteVariant.Create(Guid.NewGuid(), 0, "HIDDEN_VARIANT_SENTINEL", "oneOffPpe", null, null,
                true, Guid.NewGuid(), "PRIVATE_HIDDEN_BASIS", [
                    QuoteTier.Create("retail", 999m, true),
                    QuoteTier.Create("wholesale", 888m, true),
                    QuoteTier.Create("largeWholesale", 777m, true)])]));
        var version = QuoteVersion.Create(new QuoteId(1), 1, null, "created", "Замовник", null, null, 7, items);
        var projection = QuotePublicDocumentProjectionBuilder.Build(version, issuer, template,
            "10/10/2026", "Олена");
        var publicText = string.Join("\n", projection.Paragraphs);
        Assert.Multiple(() =>
        {
            Assert.That(publicText, Does.Not.Contain("INTERNAL_NOTE_SENTINEL"));
            Assert.That(publicText, Does.Not.Contain("PRIVATE_BASIS_SENTINEL"));
            Assert.That(publicText, Does.Not.Contain("HIDDEN_ITEM_SENTINEL"));
            Assert.That(publicText, Does.Not.Contain("HIDDEN_VARIANT_SENTINEL"));
            Assert.That(publicText, Does.Not.Contain("999.00 грн"));
            Assert.That(publicText, Does.Contain("123.45 грн"));
        });

        var workspace = Path.Combine(Path.GetTempPath(), "brevi-ts010-pdf-" + Guid.NewGuid().ToString("N"));
        try
        {
            var renderer = new ChromiumPrivateDocumentRenderer(new ChromiumDocumentOptions
            {
                ExecutablePath = executable!, FontPath = font!,
                WorkingDirectoryRootPath = Path.Combine(workspace, "private"),
                PublicWebRootPath = Path.Combine(workspace, "wwwroot")
            });
            var pdf = await renderer.RenderAsync(projection, default);
            Assert.That(pdf.AsSpan().StartsWith("%PDF-"u8), Is.True);
            var output = Environment.GetEnvironmentVariable("BREVIERP_TS010_PROOF_OUTPUT");
            if (!string.IsNullOrWhiteSpace(output)) await File.WriteAllBytesAsync(output, pdf);
        }
        finally { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
    }
}
