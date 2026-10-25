using Ardalis.Result;
using BuildingBlocks.Domain.ValueObjects;
using Catalog.Application.Features.Pricing.Sewing.Settings.CreateVersion;
using Catalog.Application.Features.Pricing.Sewing.Publications.Publish;
using Catalog.Application.Features.Pricing.Sewing.Calculations.Save;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.Pricing.History.Calculations;
using Catalog.Infrastructure.Pricing.History.Publications;
using Catalog.Infrastructure.Pricing.History.Transactions;
using Catalog.Infrastructure.Pricing.Reading;
using Catalog.Infrastructure.Pricing.SewingSettings.Writing;
using Host.Api.DependencyInjection.ServiceRegistration;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.Pricing;
using Reference.Infrastructure.Pricing.Reading;
using SewingPricing.Jobs.Bootstrap;

namespace IntegrationTests.Pricing.Sewing;

[NonParallelizable]
public sealed class SewingPricingPublicationTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Saved_calculations_are_immutable_and_only_current_reviewed_inputs_can_publish_with_history()
    {
        await PricingBootstrapJob.RunAsync(_connection, _pricingSource);
        await using var catalog = Catalog();
        await using var reference = Reference();
        var product = await catalog.Products.Include(x => x.Articles).Include(x => x.MaterialLines)
            .Include(x => x.OperationLines).AsSplitQuery().SingleAsync(x => x.ModelCode == "MOD-025");
        var article = product.Articles.Single(x => x.SaleSku == "246");
        var productId = product.Id.Value;
        var articleId = article.Id.Value;
        var transactions = new SewingPricingTransaction(catalog);
        var calculations = new SewingCalculationStore(catalog);
        var publishedReader = new Catalog.Infrastructure.Pricing.History.Publications.Reading.SewingPublishedPriceReader(catalog);
        var initialPrices = await publishedReader.ReadAsync([productId], CancellationToken.None);
        Assert.That(initialPrices.Single().ArticleCount, Is.EqualTo(2));
        Assert.That(initialPrices.Single().Articles, Is.Empty);
        var publications = new SewingPublicationStore(catalog);
        var inputReader = new SewingPricingInputReader(catalog);
        var settingsReader = new SewingPricingSettingsReader(catalog);
        var bridge = new CatalogPricingReferenceReader(new ReferencePricingVersionReader(reference), catalog, reference);
        var save = new SaveSewingCalculationCommandHandler(transactions, calculations, inputReader, settingsReader, bridge);
        var publish = new PublishSewingCalculationCommandHandler(transactions, calculations, publications, inputReader, settingsReader, bridge);
        var blockedRequest = new SaveSewingCalculationCommand(productId, articleId, Guid.NewGuid(), "manager");
        var blocked = await save.Handle(blockedRequest, CancellationToken.None);
        Assert.That(blocked.Value.Snapshot.Calculation.IsCalculated, Is.False);
        Assert.That((await publish.Handle(new(productId, articleId, blocked.Value.Id, "manager"), CancellationToken.None)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await publications.ReadCurrentAsync(productId, articleId, CancellationToken.None), Is.Null);

        product.ReplaceComposition(
            product.MaterialLines.Select(x => SewingMaterialLine.Create(CompositionLineId.Create(0), product.Id,
                x.ArticleId, x.Kind, x.FabricId, x.AccessoryId, x.CostCode, x.Quantity, x.Unit,
                x.IsRequired, x.SortOrder, true, "reviewer", DateTimeOffset.UtcNow)).ToArray(),
            product.OperationLines.Select(x => SewingOperationLine.Create(CompositionLineId.Create(0), product.Id,
                null, x.OperationId, x.Quantity, x.ManualMinutes, x.SortOrder, true, true, "reviewer", DateTimeOffset.UtcNow)).ToArray(),
            DateTimeOffset.UtcNow);
        await catalog.SaveChangesAsync();
        var replayBlocked = await save.Handle(blockedRequest, CancellationToken.None);
        Assert.That(replayBlocked.Value.Id, Is.EqualTo(blocked.Value.Id));
        Assert.That(replayBlocked.Value.Snapshot.Calculation.IsCalculated, Is.False);
        Assert.That((await save.Handle(blockedRequest with { ArticleId = product.Articles.Single(x => x.SaleSku == "247").Id.Value },
            CancellationToken.None)).Status, Is.EqualTo(ResultStatus.Conflict));

        var current = await Save();
        Assert.That(current.Snapshot.Calculation.IsCalculated, Is.True);
        Assert.That(current.Snapshot.Inputs.Materials, Is.Not.Empty);
        Assert.That(current.Snapshot.Settings!.Settings.MonthlySalary, Is.EqualTo(17500m));
        Assert.That(current.Snapshot.References, Has.Count.EqualTo(3));
        var first = await publish.Handle(new(productId, articleId, current.Id, "manager"), CancellationToken.None);
        Assert.That(first.IsSuccess, Is.True);
        Assert.That((await publish.Handle(new(productId, articleId, current.Id, "manager"), CancellationToken.None)).Value.Id,
            Is.EqualTo(first.Value.Id));

        var beforeCostChange = await Save();
        var fabric = await reference.Fabrics.SingleAsync();
        fabric.Update(fabric.Name, MoneyAmount.From(25), fabric.ProviderId);
        await reference.SaveChangesAsync();
        Assert.That(await reference.Set<ReferencePricingInputVersion>().CountAsync(x => x.FabricId == fabric.Id), Is.EqualTo(2));
        Assert.That((await publish.Handle(new(productId, articleId, beforeCostChange.Id, "manager"), CancellationToken.None)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        var afterCostChange = await Save();
        var second = await publish.Handle(new(productId, articleId, afterCostChange.Id, "manager"), CancellationToken.None);
        Assert.That(second.IsSuccess, Is.True);
        var projected = await publishedReader.ReadAsync([productId], CancellationToken.None);
        Assert.That(projected.Single().Articles.Single().PublicationId, Is.EqualTo(second.Value.Id));
        Assert.That(projected.Single().Articles.Single().Prices, Is.EqualTo(second.Value.Prices));
        Assert.That(second.Value.Prices, Is.Not.EqualTo(first.Value.Prices));
        Assert.That((await publications.FindPublicationAsync(current.Id, CancellationToken.None))!.Prices, Is.EqualTo(first.Value.Prices));
        await publish.Handle(new(productId, articleId, current.Id, "manager"), CancellationToken.None);
        Assert.That((await publications.ReadCurrentAsync(productId, articleId, CancellationToken.None))!.Id, Is.EqualTo(second.Value.Id));
        Assert.That((await calculations.FindAsync(productId, articleId, beforeCostChange.Id, CancellationToken.None))!.Snapshot.References
            .Single(x => x.ReferenceKey.StartsWith("Fabric:")).Value, Is.EqualTo(12.34m));

        var settings = afterCostChange.Snapshot.Settings!;
        var beforeSettingsChange = await Save();
        var settingsHandler = new CreateSewingSettingsVersionCommandHandler(new SewingPricingSettingsWriter(catalog));
        var settingsCommand = new CreateSewingSettingsVersionCommand(settings.Id, Guid.NewGuid(),
            settings.Settings with { MonthlySalary = 19000 }, "manager");
        var changed = await settingsHandler.Handle(settingsCommand, CancellationToken.None);
        Assert.That(changed.IsSuccess, Is.True);
        Assert.That((await publish.Handle(new(productId, articleId, beforeSettingsChange.Id, "manager"), CancellationToken.None)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await settingsHandler.Handle(settingsCommand, CancellationToken.None)).Value.Id, Is.EqualTo(changed.Value.Id));
        Assert.That((await settingsHandler.Handle(settingsCommand with { RequestId = Guid.NewGuid() }, CancellationToken.None)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await settingsHandler.Handle(settingsCommand with { Settings = settings.Settings }, CancellationToken.None)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await publish.Handle(new(productId, articleId, afterCostChange.Id, "manager"), CancellationToken.None)).Value.Id,
            Is.EqualTo(second.Value.Id), "Published retries retain the original publication even after settings change.");
        var afterSettings = await Save();
        product.ReplaceComposition(product.MaterialLines.ToArray(), product.OperationLines.ToArray(), DateTimeOffset.UtcNow);
        await catalog.SaveChangesAsync();
        Assert.That((await publish.Handle(new(productId, articleId, afterSettings.Id, "manager"), CancellationToken.None)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await calculations.FindAsync(productId, product.Articles.Single(x => x.SaleSku == "247").Id.Value,
            current.Id, CancellationToken.None), Is.Null);

        async Task<Catalog.Application.Contracts.Pricing.SavedSewingCalculation> Save()
        {
            var result = await save.Handle(new(productId, articleId, Guid.NewGuid(), "manager"), CancellationToken.None);
            Assert.That(result.IsSuccess, Is.True);
            return result.Value;
        }
    }
}
