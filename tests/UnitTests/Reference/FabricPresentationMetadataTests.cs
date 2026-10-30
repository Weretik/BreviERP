using BuildingBlocks.Domain.ValueObjects;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace UnitTests.Reference;

public sealed class FabricPresentationMetadataTests
{
    [Test]
    public void Metadata_is_optional_and_normalized_without_changing_cost()
    {
        var fabric = Fabric.Create(FabricId.From(57), "Адванта", MoneyAmount.From(150), SupplierId.From(1));
        Assert.That(fabric.Composition, Is.Null);
        Assert.That(fabric.Density, Is.Null);

        fabric.SetPresentationMetadata(" 60% бавовна, 40% поліестер ", " 235 ");

        Assert.Multiple(() =>
        {
            Assert.That(fabric.Composition, Is.EqualTo("60% бавовна, 40% поліестер"));
            Assert.That(fabric.Density, Is.EqualTo("235"));
            Assert.That(fabric.Price.Value, Is.EqualTo(150));
        });
    }

    [Test]
    public void Oversized_metadata_is_rejected()
    {
        var fabric = Fabric.Create(FabricId.From(57), "Адванта", MoneyAmount.From(150), SupplierId.From(1));

        Assert.Throws<ArgumentOutOfRangeException>(() => fabric.SetPresentationMetadata(new string('a', 501), "235"));
        Assert.Throws<ArgumentOutOfRangeException>(() => fabric.SetPresentationMetadata("бавовна", new string('a', 101)));
    }
}
