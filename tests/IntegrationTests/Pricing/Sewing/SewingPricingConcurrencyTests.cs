using Ardalis.Result;
using BuildingBlocks.Domain.ValueObjects;
using Catalog.Application.Features.Pricing.Sewing.Publications.Publish;
using Catalog.Application.Features.Pricing.Sewing.Calculations.Save;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.Pricing.History.Calculations;
using Catalog.Infrastructure.Pricing.History.Publications;
using Catalog.Infrastructure.Pricing.History.Transactions;
using Catalog.Infrastructure.Pricing.Reading;
using Host.Api.DependencyInjection.ServiceRegistration;
using Microsoft.EntityFrameworkCore;
using Reference.Infrastructure.Pricing.Reading;
using SewingPricing.Jobs.Bootstrap;

namespace IntegrationTests.Pricing.Sewing;

[NonParallelizable]
public sealed class SewingPricingConcurrencyTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Publication_waits_for_a_concurrent_cost_edit_and_checks_its_committed_version()
    {
        await PricingBootstrapJob.RunAsync(_connection, _pricingSource);
        await using var catalog = Catalog();
        await using var reference = Reference();
        var product = await catalog.Products.Include(x => x.Articles).Include(x => x.MaterialLines)
            .Include(x => x.OperationLines).AsSplitQuery().SingleAsync(x => x.ModelCode == "MOD-025");
        product.ReplaceComposition(
            product.MaterialLines.Select(x => SewingMaterialLine.Create(CompositionLineId.Create(0), product.Id,
                x.ArticleId, x.Kind, x.FabricId, x.AccessoryId, x.CostCode, x.Quantity, x.Unit,
                x.IsRequired, x.SortOrder, true, "reviewer", DateTimeOffset.UtcNow)).ToArray(),
            product.OperationLines.Select(x => SewingOperationLine.Create(CompositionLineId.Create(0), product.Id,
                null, x.OperationId, x.Quantity, x.ManualMinutes, x.SortOrder, true, true, "reviewer", DateTimeOffset.UtcNow)).ToArray(),
            DateTimeOffset.UtcNow);
        await catalog.SaveChangesAsync();
        var article = product.Articles.First();
        var transactions = new SewingPricingTransaction(catalog);
        var calculations = new SewingCalculationStore(catalog);
        var publications = new SewingPublicationStore(catalog);
        var inputs = new SewingPricingInputReader(catalog);
        var settings = new SewingPricingSettingsReader(catalog);
        var bridge = new CatalogPricingReferenceReader(new ReferencePricingVersionReader(reference), catalog, reference);
        var saved = await new SaveSewingCalculationCommandHandler(transactions, calculations, inputs, settings, bridge)
            .Handle(new(product.Id.Value, article.Id.Value, Guid.NewGuid(), "manager"), CancellationToken.None);
        Assert.That(saved.Value.Snapshot.Calculation.IsCalculated, Is.True);

        await using var writer = Reference();
        await using var transaction = await writer.Database.BeginTransactionAsync();
        await writer.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025004)");
        var fabric = await writer.Fabrics.SingleAsync();
        fabric.Update(fabric.Name, MoneyAmount.From(50), fabric.ProviderId);
        await writer.SaveChangesAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var publishing = new PublishSewingCalculationCommandHandler(transactions, calculations, publications, inputs, settings, bridge)
            .Handle(new(product.Id.Value, article.Id.Value, saved.Value.Id, "manager"), timeout.Token).AsTask();
        try
        {
            Assert.That(await Task.WhenAny(publishing, Task.Delay(200)), Is.Not.SameAs(publishing),
                "Publication must wait for the pricing write lock.");
        }
        finally
        {
            await transaction.CommitAsync();
        }
        Assert.That((await publishing).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await publications.ReadCurrentAsync(product.Id.Value, article.Id.Value, CancellationToken.None), Is.Null);
    }
}
