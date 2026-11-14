using Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Store;
using IntegrationTests.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing;

namespace IntegrationTests.Pricing.Ppe;

/// <summary>«Імпорт цін» settings and runs in PostgreSQL (CAT-017).</summary>
[NonParallelizable]
public sealed class PpePriceSourceStoreTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Settings_are_upserted_and_only_the_last_run_of_each_source_is_read()
    {
        var at = new DateTimeOffset(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);
        await using (var db = Catalog())
        {
            var store = new PpePriceSourceStore(db);
            await store.SaveSettingsAsync(new PpePriceSourceSettings("av-center", null, 20m, at, "admin@brevi"), CancellationToken.None);
            await store.SaveSettingsAsync(new PpePriceSourceSettings("av-center", 1, 18.5m, at.AddMinutes(1), "director@brevi"), CancellationToken.None);
            await store.RecordRunAsync(Run("cemto", at, 3), CancellationToken.None);
            await store.RecordRunAsync(Run("cemto", at.AddHours(1), 1, new PpePriceImportProblem("1003", "Черевики", "Товар не знайдено у файлі.")), CancellationToken.None);
            await store.RecordRunAsync(Run("insight", at, 26), CancellationToken.None);
        }

        await using var read = Catalog();
        var reader = new PpePriceSourceStore(read);
        var settings = (await reader.ReadSettingsAsync(CancellationToken.None)).Single();
        var runs = (await reader.ReadLastRunsAsync(CancellationToken.None)).ToDictionary(x => x.SourceKey);
        Assert.Multiple(() =>
        {
            Assert.That((settings.SupplierId, settings.DiscountPercent, settings.UpdatedBy), Is.EqualTo(((int?)1, 18.5m, "director@brevi")));
            Assert.That(runs.Keys, Is.EquivalentTo(new[] { "cemto", "insight" }));
            Assert.That(runs["cemto"].Changed, Is.EqualTo(1));
            Assert.That(runs["cemto"].Problems.Single().Message, Is.EqualTo("Товар не знайдено у файлі."));
        });
    }

    private static PpePriceImportRun Run(string key, DateTimeOffset at, int changed, params PpePriceImportProblem[] problems)
        => new(key, "Файл", "manager@brevi", at, changed + 1, changed, 1, 0, problems);
}
