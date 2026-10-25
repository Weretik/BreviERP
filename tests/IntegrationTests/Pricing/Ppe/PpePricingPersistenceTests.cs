using System.Text.Json;
using Ardalis.Result;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Products;
using Catalog.Infrastructure.Pricing.Ppe.Reading;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using Catalog.Infrastructure.Pricing.Ppe.Writing;
using CatalogImport.PpePricing.Bootstrap;
using Catalog.Domain.Products.Enums;
using IntegrationTests.Pricing.Sewing;
using Microsoft.EntityFrameworkCore;
namespace IntegrationTests.Pricing.Ppe;
[NonParallelizable]
public sealed class PpePricingPersistenceTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Draft_baseline_replays_and_approved_versions_produce_current_prices_without_overwriting_history()
    {
        await using (var upgrade=Catalog())
        {
            var migrations=upgrade.Database.GetMigrations().ToArray();
            var migrator=Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>(upgrade);
            await migrator.MigrateAsync(migrations[^2]);
            await migrator.MigrateAsync();
            Assert.That(await upgrade.Products.CountAsync(),Is.GreaterThan(0));
        }
        var path=Path.Combine(TestContext.CurrentContext.WorkDirectory,$"ppe-{Guid.NewGuid():N}.json");
        try
        {
            var row=new object?[18]; row[0]="724";row[7]=100m;row[11]=true;row[12]="2026-10-25";row[13]="Rule";
            await File.WriteAllTextAsync(path,JsonSerializer.Serialize(new { version=1,spreadsheetId="test-workbook",capturedAt="2026-10-25",
                rules=new object?[][] { ["Code","Retail","Wholesale","Large"],["Rule",.5m,.36m,.3m] }, products=new object?[][] { ["Sku"],row } }));
            Assert.That((await PpePricingBootstrapJob.RunAsync(_connection,path)).Status,Is.EqualTo("applied-drafts"));
            Assert.That((await PpePricingBootstrapJob.RunAsync(_connection,path)).Status,Is.EqualTo("already-applied"));
            await using var db=Catalog();
            var id=(await db.Products.SingleAsync(x=>x.Type==ProductType.Ppe)).Id.Value;
            var reader=new PpePricingInputReader(db);
            var query=new GetPpeProductPricesQueryHandler(reader);
            var draft=(await query.Handle(new([id]),CancellationToken.None)).Value.Single();
            Assert.That(draft.Calculation.Prices,Is.Null);
            Assert.That(draft.Offer!.Values.SourceReviewed,Is.True);
            Assert.That(draft.Offer.IsApproved,Is.False);
            var ruleWriter=new PpeRuleVersionWriter(db);var offerWriter=new PpeOfferVersionWriter(db);
            var rule=await ruleWriter.AppendAsync(draft.Rule!.Id,Guid.NewGuid(),draft.Rule.Values,"reviewer",CancellationToken.None);
            Assert.That(rule.IsSuccess,Is.True);
            var request=Guid.NewGuid();
            var approved=await offerWriter.AppendAsync(id,draft.Offer.Id,request,draft.Offer.Values,"reviewer",CancellationToken.None);
            Assert.That(approved.IsSuccess,Is.True);
            Assert.That((await offerWriter.AppendAsync(id,draft.Offer.Id,request,draft.Offer.Values,"reviewer",CancellationToken.None)).Value.Id,Is.EqualTo(approved.Value.Id));
            Assert.That((await offerWriter.AppendAsync(id,draft.Offer.Id,request,draft.Offer.Values,"other",CancellationToken.None)).Status,Is.EqualTo(ResultStatus.Conflict));
            Assert.That((await offerWriter.AppendAsync(id,draft.Offer.Id,Guid.NewGuid(),draft.Offer.Values,"reviewer",CancellationToken.None)).Status,Is.EqualTo(ResultStatus.Conflict));
            var current=(await query.Handle(new([id,999999]),CancellationToken.None)).Value.Single();
            Assert.That(current.Calculation.Prices!.Wholesale,Is.EqualTo(136));
            Assert.That(current.Offer!.Id,Is.EqualTo(approved.Value.Id));
            var external=draft.Offer.Values with { ExternalRetail=237,ExternalWholesale=187.9m,RuleCode="missing",SupplierCost=null };
            var changed=await offerWriter.AppendAsync(id,approved.Value.Id,Guid.NewGuid(),external,"reviewer",CancellationToken.None);
            Assert.That(changed.IsSuccess,Is.True);
            Assert.That((await query.Handle(new([id]),CancellationToken.None)).Value.Single().Calculation.Prices!.Wholesale,Is.EqualTo(212.45m));
            Assert.That(await db.Set<PpeOfferVersionRecord>().CountAsync(),Is.EqualTo(3));
            Assert.That((await db.Set<PpeOfferVersionRecord>().AsNoTracking().SingleAsync(x=>x.PublicId==approved.Value.Id)).Values().SupplierCost,Is.EqualTo(100));
            Assert.That((await PpePricingBootstrapJob.RunAsync(_connection,path)).Status,Is.EqualTo("already-applied"));
            row[7]=200m;
            await File.WriteAllTextAsync(path,JsonSerializer.Serialize(new { version=1,spreadsheetId="test-workbook",capturedAt="2026-10-25",
                rules=new object?[][] { ["Code","Retail","Wholesale","Large"],["Rule",.5m,.36m,.3m] }, products=new object?[][] { ["Sku"],row } }));
            Assert.ThrowsAsync<InvalidOperationException>(()=>PpePricingBootstrapJob.RunAsync(_connection,path));
        }
        finally { File.Delete(path); }
    }
}
