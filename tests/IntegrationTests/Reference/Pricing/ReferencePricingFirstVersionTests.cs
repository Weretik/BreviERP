using IntegrationTests.TestSupport;
using BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.Pricing;
using Reference.Domain.Suppliers;

namespace IntegrationTests.Reference.Pricing;

/// <summary>CAT-011: a new reference gets its first pricing input version; a deleted one takes its versions along.</summary>
[NonParallelizable]
public sealed class ReferencePricingFirstVersionTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task New_fabric_and_accessory_get_a_first_version_then_next_ones_and_lose_them_on_delete()
    {
        await using var reference = Reference();
        var fabricId = FabricId.From(9001);
        var accessoryId = GarmentAccessoryId.From(9001);
        reference.Fabrics.Add(Fabric.Create(fabricId, "Тест тканина", MoneyAmount.From(120), SupplierId.From(1)).Value);
        reference.GarmentAccessories.Add(GarmentAccessory.Create(accessoryId, "Тест фурнітура", MoneyAmount.From(3)).Value);
        await reference.SaveChangesAsync();

        var fabricVersion = await reference.Set<ReferencePricingInputVersion>().AsNoTracking().SingleAsync(x => x.FabricId == fabricId);
        Assert.That((fabricVersion.Revision, fabricVersion.Value, fabricVersion.Unit, fabricVersion.IsApproved, fabricVersion.SourceCode),
            Is.EqualTo((1, 120m, PricingInputUnit.UahPerMetre, true, "reference:Fabric:9001")));
        var accessoryVersion = await reference.Set<ReferencePricingInputVersion>().AsNoTracking().SingleAsync(x => x.AccessoryId == accessoryId);
        Assert.That((accessoryVersion.Revision, accessoryVersion.Unit), Is.EqualTo((1, PricingInputUnit.UahPerPiece)));

        var fabric = await reference.Fabrics.SingleAsync(x => x.Id == fabricId);
        fabric.Update(fabric.Name + " (нова назва)", fabric.Price, fabric.ProviderId);
        await reference.SaveChangesAsync();
        Assert.That(await reference.Set<ReferencePricingInputVersion>().CountAsync(x => x.FabricId == fabricId), Is.EqualTo(1),
            "A change that keeps the price needs no new version.");
        fabric.Update(fabric.Name, MoneyAmount.From(150), fabric.ProviderId);
        await reference.SaveChangesAsync();
        var latest = await reference.Set<ReferencePricingInputVersion>().AsNoTracking()
            .Where(x => x.FabricId == fabricId).OrderByDescending(x => x.Revision).FirstAsync();
        Assert.That((latest.Revision, latest.Value, latest.SourceCode), Is.EqualTo((2, 150m, "reference:Fabric:9001")));

        reference.Fabrics.Remove(fabric);
        reference.GarmentAccessories.Remove(await reference.GarmentAccessories.SingleAsync(x => x.Id == accessoryId));
        await reference.SaveChangesAsync();
        Assert.That(await reference.Set<ReferencePricingInputVersion>()
            .CountAsync(x => x.FabricId == fabricId || x.AccessoryId == accessoryId), Is.Zero);
    }
}
