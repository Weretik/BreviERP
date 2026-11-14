using Catalog.Domain.Products;
using IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Moq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Running;
using Catalog.Infrastructure.Integrations.PpeSupplierFiles;
using Catalog.Infrastructure.Persistence;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Ppe;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.Persistence.Configurations.Pricing.Ppe;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.Infrastructure.Persistence.Repositories.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing.PpeSupplierPrices.Sources;
using Catalog.Application.Abstractions.Reference.Items;

namespace IntegrationTests.Catalog.Pricing.Ppe.SupplierPrices;

/// <summary>A СЕМТО PDF previewed and applied in PostgreSQL: new card price, new offer version, run recorded (CAT-017).</summary>
[NonParallelizable]
public sealed class PpeSupplierFileImportTests : SewingPricingDatabaseFixture
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task Preview_changes_nothing_and_apply_saves_the_new_price_as_an_offer_version()
    {
        await using (var seed = Catalog())
        {
            seed.Products.Add(Ppe(910001, "1003", "8011", 600m, manual: false));
            seed.Products.Add(Ppe(910002, "1004", "8010", 870.78m, manual: true));
            await seed.SaveChangesAsync();
        }

        var preview = await Import(dryRun: true);
        Assert.That(preview.IsSuccess, Is.True, string.Join("; ", preview.ValidationErrors.Select(x => x.ErrorMessage)));
        Assert.That(preview.Value.Rows.Select(x => (x.Code, x.Status)), Is.EqualTo(new[]
        {
            ("1003", PpePriceImportStatus.Changed), ("1004", PpePriceImportStatus.ManualPrice)
        }));
        await using (var check = Catalog())
            Assert.That(await check.Set<PpeOfferVersionRecord>().CountAsync(), Is.Zero);

        var applied = await Import(dryRun: false);
        Assert.That(applied.Value.Changed, Is.EqualTo(1));

        await using var db = Catalog();
        var product = await db.Products.Include(x => x.CatalogProfile).SingleAsync(x => x.Id == ProductId.Create(910001));
        var offer = (await db.Set<PpeOfferVersionRecord>().AsNoTracking().SingleAsync()).Values();
        var run = (await new PpePriceSourceStore(db).ReadLastRunsAsync(CancellationToken.None)).Single();
        Assert.Multiple(() =>
        {
            Assert.That(product.CatalogProfile!.SupplierPrice, Is.EqualTo(620.22m));
            Assert.That((offer.SupplierCost, offer.Source), Is.EqualTo(((decimal?)620.22m, "Імпорт цін: СЕМТО")));
            Assert.That((run.SourceKey, run.Changed, run.Skipped, run.Actor), Is.EqualTo(("cemto", 1, 1, "manager@brevi")));
        });
        Assert.That((await Import(dryRun: true)).Value.Rows.First().Status, Is.EqualTo(PpePriceImportStatus.Unchanged));
    }

    private async Task<Ardalis.Result.Result<PpePriceImportResult>> Import(bool dryRun)
    {
        await using var db = Catalog();
        var references = new Mock<IProductReferenceReader>();
        references.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ProductReferenceData(
            new Dictionary<int, ReferenceItem> { [1] = new(1, "СЕМТО") }, new Dictionary<int, PricedReferenceItem>(),
            new Dictionary<int, PricedReferenceItem>(), new Dictionary<int, OperationReferenceItem>(),
            new Dictionary<int, AdditionalReferenceItem>()));
        var handler = new ImportPpeSupplierFilesHandler(new PpeSupplierFileReader(), new PpePriceImportRunner(
            new PpePriceSourceStore(db), references.Object, new CatalogEfRepository<Product>(db),
            new PpePricingInputReader(db), new PpeOfferVersionWriter(db), TimeProvider.System));
        return await handler.Handle(new ImportPpeSupplierFilesCommand("cemto", [CemtoPdf()], dryRun, "manager-id", "manager@brevi"),
            CancellationToken.None);
    }

    private static Product Ppe(int id, string code, string sku, decimal price, bool manual)
    {
        var product = Product.Create(ProductId.Create(id), "Черевики " + sku, "Черевики " + sku,
            ProductSlug.Create("cherevyky-" + sku).Value, ProductType.Ppe, Now).Value;
        product.SetCode(code, Now);
        product.SetPpeSupplier(new PpeSupplierDetails(SupplierReferenceId.Create(1), sku, null, price, manual, "Взуття", "пара"), Now);
        return product;
    }

    private static SupplierPriceFile CemtoPdf()
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var page = builder.AddPage(PageSize.A4);
        page.AddText("8011", 8, new PdfPoint(67, 700), font);
        page.AddText("620,22", 8, new PdfPoint(244, 700), font);
        page.AddText("8010", 8, new PdfPoint(67, 600), font);
        page.AddText("900,00", 8, new PdfPoint(244, 600), font);
        return new SupplierPriceFile("cemto.pdf", new MemoryStream(builder.Build()));
    }
}
