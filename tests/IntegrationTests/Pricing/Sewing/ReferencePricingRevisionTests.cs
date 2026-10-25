using Ardalis.Result;
using BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Reference.Application.Contracts.Pricing;
using Reference.Application.Features.Pricing.ReviseOtherCost;
using Reference.Domain.Pricing;
using Reference.Infrastructure.Pricing.OtherCosts.Writing;
using SewingPricing.Jobs.Bootstrap;

namespace IntegrationTests.Pricing.Sewing;

[NonParallelizable]
public sealed class ReferencePricingRevisionTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Reference_edits_append_atomically_and_reject_stale_and_future_revisions()
    {
        await using var reference = Reference();
        await reference.GetService<IMigrator>().MigrateAsync("20261025102037_AddReferencePricingInputVersions");
        await reference.Database.MigrateAsync();
        Assert.That((await reference.GarmentPartOperations.AsNoTracking().SingleAsync()).Min, Is.EqualTo(3));
        await PricingBootstrapJob.RunAsync(_connection, _pricingSource);
        await using var stale = Reference();
        var originalFabric = await stale.Fabrics.SingleAsync();
        var fabric = await reference.Fabrics.SingleAsync();
        fabric.Update(fabric.Name + " renamed", fabric.Price, fabric.ProviderId);
        await reference.SaveChangesAsync();
        Assert.That(await reference.Set<ReferencePricingInputVersion>().CountAsync(), Is.EqualTo(3));
        fabric.Update(fabric.Name, MoneyAmount.From(20), fabric.ProviderId);
        await reference.SaveChangesAsync();
        originalFabric.Update(originalFabric.Name, MoneyAmount.From(30), originalFabric.ProviderId);
        Assert.ThrowsAsync<ReferencePricingConflictException>(async () => await stale.SaveChangesAsync());
        await using (var check = Reference())
        {
            Assert.That((await check.Fabrics.SingleAsync()).Price.Value, Is.EqualTo(20));
            Assert.That(await check.Set<ReferencePricingInputVersion>().CountAsync(x => x.FabricId != null), Is.EqualTo(2));
        }

        var accessory = await reference.GarmentAccessories.SingleAsync();
        var operation = await reference.GarmentPartOperations.SingleAsync();
        accessory.Update(accessory.Name, MoneyAmount.From(4), accessory.SupplierId.Value);
        operation.Update(operation.GarmentPartId, operation.Name, 7.12345678m);
        await reference.SaveChangesAsync();
        Assert.That(await reference.Set<ReferencePricingInputVersion>().CountAsync(), Is.EqualTo(6));
        await using (var precision = Reference())
            Assert.That((await precision.GarmentPartOperations.SingleAsync()).Min, Is.EqualTo(7.12345678m));

        var latest = await reference.Set<ReferencePricingInputVersion>().AsNoTracking()
            .Where(x => x.OperationId != null).OrderByDescending(x => x.Revision).FirstAsync();
        reference.Add(ReferencePricingInputVersion.Create(null, null, latest.OperationId, null, 8, latest.Unit,
            true, true, latest.Revision + 1, DateTimeOffset.UtcNow.AddDays(1), latest.SourceCode, new string('D', 64), null));
        await reference.SaveChangesAsync();
        operation.Update(operation.GarmentPartId, operation.Name, 9);
        Assert.Throws<InvalidOperationException>(() => reference.SaveChanges());
        Assert.ThrowsAsync<ReferencePricingConflictException>(async () => await reference.SaveChangesAsync());
        await using var final = Reference();
        Assert.That((await final.GarmentPartOperations.SingleAsync()).Min, Is.EqualTo(7.12345678m));
    }

}
