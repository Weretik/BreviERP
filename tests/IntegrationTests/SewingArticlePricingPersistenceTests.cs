using System.Security.Cryptography;
using System.Text.Json;
using Catalog.Application.Features.Pricing.Sewing.Preview;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Reading;
using CatalogImport.Execution;
using CatalogImport.Persistence;
using SewingPricing.Jobs.Bootstrap;
using SewingPricing.Jobs.Reading;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reference.Application.Contracts.Pricing;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.Pricing;
using Reference.Domain.Suppliers.Entities;
using Reference.Domain.Suppliers.ValueObjects;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Pricing.Reading;

using IntegrationTests.Pricing.Sewing;

namespace IntegrationTests;

[NonParallelizable]
public sealed class SewingArticlePricingPersistenceTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Bootstrap_is_repeatable_and_effective_versions_preserve_review_and_ambiguity_gates()
    {
        var first = await PricingBootstrapJob.RunAsync(_connection, _pricingSource);
        var repeat = await PricingBootstrapJob.RunAsync(_connection, _pricingSource);
        Assert.Multiple(() =>
        {
            Assert.That(first.Status, Is.EqualTo("bootstrapped"));
            Assert.That(first.ReferenceVersions, Is.EqualTo(3));
            Assert.That(repeat.Status, Is.EqualTo("already-bootstrapped"));
            Assert.That(repeat.SettingsVersionId, Is.EqualTo(first.SettingsVersionId));
        });
        await using var catalog = Catalog();
        await using var reference = Reference();
        var product = await catalog.Products.Include(x => x.Articles).Include(x => x.MaterialLines)
            .Include(x => x.OperationLines).AsSplitQuery().SingleAsync(x => x.ModelCode == "MOD-025");
        var article = product.Articles.Single(x => x.SaleSku == "246");
        var reader = new ReferencePricingVersionReader(reference);
        var handler = new CalculateSewingArticleQueryHandler(new SewingPricingInputReader(catalog),
            new SewingPricingSettingsReader(catalog), new JobPricingReferenceReader(reader));
        var query = new CalculateSewingArticleQuery(product.Id.Value, article.Id.Value, EffectiveFrom);
        var blocked = await handler.Handle(query, CancellationToken.None);
        Assert.That(blocked.Value.Calculation.Blockers.Select(x => x.Code), Does.Contain("UnreviewedOperation"));
        Assert.That(blocked.Value.Calculation.Prices, Is.Null);

        product.ReplaceComposition(
            product.MaterialLines.Select(x => SewingMaterialLine.Create(CompositionLineId.Create(0), product.Id,
                x.ArticleId, x.Kind, x.FabricId, x.AccessoryId, x.CostCode, x.Quantity, x.Unit,
                x.IsRequired, x.SortOrder, true, "test-reviewer", EffectiveFrom)).ToArray(),
            product.OperationLines.Select(x => SewingOperationLine.Create(CompositionLineId.Create(0), product.Id,
                null, x.OperationId, x.Quantity, x.ManualMinutes, x.SortOrder, true, true, "test-reviewer", EffectiveFrom)).ToArray(),
            EffectiveFrom);
        await catalog.SaveChangesAsync();
        var calculated = await handler.Handle(query, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(calculated.Value.Calculation.IsCalculated, Is.True);
            Assert.That(calculated.Value.Calculation.Breakdown!.MaterialsTotal, Is.EqualTo(49.174m));
            Assert.That(calculated.Value.Calculation.Breakdown.TotalMinutes, Is.EqualTo(192m));
            Assert.That(calculated.Value.ReferenceVersionIds, Has.Count.EqualTo(3));
        });
        var input = await reference.Set<ReferencePricingInputVersion>().SingleAsync(x => x.FabricId != null);
        var sameInstant = await handler.Handle(query with { At = EffectiveFrom.ToOffset(TimeSpan.FromHours(2)) }, CancellationToken.None);
        Assert.That(sameInstant.Value.Calculation.Prices, Is.EqualTo(calculated.Value.Calculation.Prices));
        Assert.That(await new ReferencePricingUsageReader(reference).IsFabricVersionedAsync(input.FabricId!.Value.Value, CancellationToken.None), Is.True);
        var future = EffectiveFrom.AddDays(1);
        reference.Add(ReferencePricingInputVersion.Create(input.FabricId, null, null, null, 99m, input.Unit,
            true, true, 2, future, input.SourceCode, new string('B', 64), null));
        await reference.SaveChangesAsync();
        var request = new PricingReferenceRequest([input.FabricId!.Value.Value], [], [], []);
        Assert.That((await reader.ReadAsync(request, EffectiveFrom, CancellationToken.None)).Single().Value, Is.EqualTo(12.34m));
        Assert.That((await reader.ReadAsync(request, future, CancellationToken.None)).Single().Value, Is.EqualTo(99m));
        reference.Add(ReferencePricingInputVersion.Create(input.FabricId, null, null, null, 97m, input.Unit,
            true, true, 3, future, input.SourceCode, new string('C', 64), null));
        await reference.SaveChangesAsync();
        var ambiguous = await handler.Handle(query with { At = future }, CancellationToken.None);
        Assert.That(ambiguous.Value.Calculation.Blockers.Select(x => x.Code), Does.Contain("AmbiguousReferenceVersion"));

        var settings = await catalog.Set<SewingPricingSettingsVersion>().SingleAsync();
        catalog.Add(SewingPricingSettingsVersion.Create(2, future, settings.ToSettings(), new string('B', 64), "{}", true));
        catalog.Add(SewingPricingSettingsVersion.Create(3, future, settings.ToSettings(), new string('C', 64), "{}", true));
        await catalog.SaveChangesAsync();
        var settingRows = await new SewingPricingSettingsReader(catalog).ReadCandidatesAsync(EffectiveFrom, CancellationToken.None);
        Assert.That(settingRows.Single().Revision, Is.EqualTo(1));
        var ambiguousSettings = await handler.Handle(query with { At = future }, CancellationToken.None);
        Assert.That(ambiguousSettings.Value.Calculation.Blockers.Select(x => x.Code), Does.Contain("AmbiguousSettings"));
    }

}
