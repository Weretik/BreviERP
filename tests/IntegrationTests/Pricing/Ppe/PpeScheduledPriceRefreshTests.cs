using Ardalis.Result;
using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Refresh;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Running;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;
using Catalog.Domain.Products;
using Catalog.Infrastructure.Pricing.Ppe.Reading;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Store;
using Catalog.Infrastructure.Pricing.Ppe.Writing;
using Catalog.Infrastructure.Repositories;
using IntegrationTests.Pricing.Sewing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegrationTests.Pricing.Ppe;

/// <summary>
/// The Monday step of brevierp-sync (Mon–Fri 09:00 Kyiv) in PostgreSQL: a connected source saves its changed prices as «Розклад», a source
/// that cannot be read records its reason, and other days run nothing (CAT-017 stages 2–3).
/// </summary>
[NonParallelizable]
public sealed class PpeScheduledPriceRefreshTests : SewingPricingDatabaseFixture
{
    private static readonly DateTimeOffset Monday = new(2026, 11, 2, 5, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Wednesday = new(2026, 11, 4, 5, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task Monday_saves_3c_prices_and_records_the_sources_that_cannot_be_read()
    {
        await using (var seed = Catalog())
        {
            seed.Products.Add(Ppe(920001, "2001", "0020", 100m));
            await seed.SaveChangesAsync();
        }

        await using var db = Catalog();
        var outcomes = await Refresh(db, Monday).RunDueAsync("nightly-sync", CancellationToken.None);

        var product = await db.Products.AsNoTracking().Include(x => x.CatalogProfile)
            .SingleAsync(x => x.Id == ProductId.Create(920001));
        var offer = (await db.Set<PpeOfferVersionRecord>().AsNoTracking().SingleAsync()).Values();
        var runs = (await new PpePriceSourceStore(db).ReadLastRunsAsync(CancellationToken.None)).ToDictionary(x => x.SourceKey);
        Assert.Multiple(() =>
        {
            Assert.That(outcomes.Select(x => x.SourceKey), Is.EquivalentTo(new[] { "av-center", "3c-market", "perchatka-centr", "trele" }));
            Assert.That(product.CatalogProfile!.SupplierPrice, Is.EqualTo(190m));
            Assert.That(offer.Source, Is.EqualTo("Імпорт цін: 3C-Маркет"));
            Assert.That((runs["3c-market"].Trigger, runs["3c-market"].Changed, runs["3c-market"].Actor),
                Is.EqualTo(("Розклад", 1, "nightly-sync")));
            Assert.That(runs["av-center"].Problems.Single().Message, Does.Contain("не знайдено"));
            Assert.That(outcomes.Single(x => x.SourceKey == "av-center").Error, Is.Not.Null);
        });
    }

    [Test]
    public void Other_days_run_nothing()
    {
        using var db = Catalog();
        Assert.That(Refresh(db, Wednesday).DueToday(), Is.Empty);
    }

    [Test]
    public async Task Daily_job_resolves_the_step_services()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=unused"
        }).Build();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        Host.Seed.SeedHostServicesExtensions.AddSeedHostServices(services, configuration);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        Assert.That(scope.ServiceProvider.GetRequiredService<PpeScheduledPriceRefresh>(), Is.Not.Null);
    }

    private static PpeScheduledPriceRefresh Refresh(Catalog.Infrastructure.DataBase.CatalogDbContext db, DateTimeOffset now)
    {
        var references = new Mock<IProductReferenceReader>();
        references.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ProductReferenceData(
            new Dictionary<int, ReferenceItem> { [1] = new(1, "3C-Макркет") }, new Dictionary<int, PricedReferenceItem>(),
            new Dictionary<int, PricedReferenceItem>(), new Dictionary<int, OperationReferenceItem>(),
            new Dictionary<int, AdditionalReferenceItem>()));
        var reader = new Mock<IPpeSupplierOnlinePriceReader>();
        reader.Setup(x => x.ReadAsync(It.IsAny<PpePriceSource>(), It.IsAny<IReadOnlyList<PpePriceImportProduct>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SupplierFilePrice>>.Success(
                [new SupplierFilePrice("20", null, null, 200m, null, null, null)]));
        var time = new FixedTime(now);
        var store = new PpePriceSourceStore(db);
        return new PpeScheduledPriceRefresh(reader.Object, new PpePriceImportRunner(store, references.Object,
            new CatalogEfRepository<Product>(db), new PpePricingInputReader(db), new PpeOfferVersionWriter(db), time),
            store, time);
    }

    private static Product Ppe(int id, string code, string sku, decimal price)
    {
        var product = Product.Create(ProductId.Create(id), "Костюм " + code, "Костюм " + code,
            ProductSlug.Create("kostium-" + code).Value, ProductType.Ppe, Monday).Value;
        product.SetCode(code, Monday);
        product.SetPpeSupplier(new PpeSupplierDetails(SupplierReferenceId.Create(1), sku, null, price, false, "Одяг ЗІЗ", "шт"), Monday);
        return product;
    }

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
