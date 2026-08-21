using Ardalis.Specification;
using Moq;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PricingReference.GetByIds;
using Reference.Domain.AdditionalReferences.Entities;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;

namespace UnitTests;

public sealed class PricingReferenceQueryHandlerTests
{
    [Test]
    public async Task Returns_only_requested_pricing_inputs_without_ui_lookups()
    {
        var fabrics = new Mock<IReferenceReadRepository<Fabric>>();
        var accessories = new Mock<IReferenceReadRepository<GarmentAccessory>>();
        var operations = new Mock<IReferenceReadRepository<GarmentPartOperation>>();
        var additionalReferences = new Mock<IReferenceReadRepository<AdditionalReference>>();
        fabrics.Setup(x => x.ListAsync(It.IsAny<ISpecification<Fabric, PricingReferenceItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PricingReferenceItem(10, 100m)]);
        accessories.Setup(x => x.ListAsync(It.IsAny<ISpecification<GarmentAccessory, PricingReferenceItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        operations.Setup(x => x.ListAsync(It.IsAny<ISpecification<GarmentPartOperation, PricingReferenceItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PricingReferenceItem(20, 15m)]);
        additionalReferences.Setup(x => x.ListAsync(It.IsAny<ISpecification<AdditionalReference, PricingReferenceItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PricingReferenceItem(30, 10m, "profit_10", "%")]);
        var handler = new GetPricingReferencesByIdsQueryHandler(
            fabrics.Object,
            accessories.Object,
            operations.Object,
            additionalReferences.Object);

        var result = await handler.Handle(
            new GetPricingReferencesByIdsQuery([10], [], [20], [30], ["profit_10"]),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Fabrics, Is.EqualTo([new PricingReferenceItem(10, 100m)]));
            Assert.That(result.GarmentAccessories, Is.Empty);
            Assert.That(result.GarmentPartOperations, Is.EqualTo([new PricingReferenceItem(20, 15m)]));
            Assert.That(result.AdditionalReferences, Is.EqualTo([new PricingReferenceItem(30, 10m, "profit_10", "%")]));
        });
        fabrics.Verify(x => x.ListAsync(It.IsAny<ISpecification<Fabric, PricingReferenceItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        accessories.Verify(x => x.ListAsync(It.IsAny<ISpecification<GarmentAccessory, PricingReferenceItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        operations.Verify(x => x.ListAsync(It.IsAny<ISpecification<GarmentPartOperation, PricingReferenceItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        additionalReferences.Verify(x => x.ListAsync(It.IsAny<ISpecification<AdditionalReference, PricingReferenceItem>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
