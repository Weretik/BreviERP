using Ardalis.Specification;
using Moq;
using Reference.Domain.PricingParameters;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Application.Abstractions;
using Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds;

namespace UnitTests.Reference.Pricing;

public sealed class PricingReferenceQueryHandlerTests
{
    [Test]
    public async Task Returns_only_requested_pricing_inputs_without_ui_lookups()
    {
        var fabrics = new Mock<IReferenceReadRepository<Fabric>>();
        var accessories = new Mock<IReferenceReadRepository<GarmentAccessory>>();
        var operations = new Mock<IReferenceReadRepository<GarmentPartOperation>>();
        var pricingParameters = new Mock<IReferenceReadRepository<PricingParameter>>();
        fabrics.Setup(x => x.ListAsync(It.IsAny<ISpecification<Fabric, PricingReferenceItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PricingReferenceItem(10, 100m)]);
        accessories.Setup(x => x.ListAsync(It.IsAny<ISpecification<GarmentAccessory, PricingReferenceItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        operations.Setup(x => x.ListAsync(It.IsAny<ISpecification<GarmentPartOperation, PricingReferenceItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PricingReferenceItem(20, 15m)]);
        pricingParameters.Setup(x => x.ListAsync(It.IsAny<ISpecification<PricingParameter, PricingReferenceItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PricingReferenceItem(30, 10m, "SEWING_PRICE_COEF_1", "%")]);
        var handler = new GetPricingReferencesByIdsHandler(
            fabrics.Object,
            accessories.Object,
            operations.Object,
            pricingParameters.Object);

        var result = await handler.Handle(
            new GetPricingReferencesByIdsQuery([10], [], [20], [30], ["SEWING_PRICE_COEF_1"]),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Fabrics, Is.EqualTo([new PricingReferenceItem(10, 100m)]));
            Assert.That(result.GarmentAccessories, Is.Empty);
            Assert.That(result.GarmentPartOperations, Is.EqualTo([new PricingReferenceItem(20, 15m)]));
            Assert.That(result.PricingParameters, Is.EqualTo([new PricingReferenceItem(30, 10m, "SEWING_PRICE_COEF_1", "%")]));
        });
        fabrics.Verify(x => x.ListAsync(It.IsAny<ISpecification<Fabric, PricingReferenceItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        accessories.Verify(x => x.ListAsync(It.IsAny<ISpecification<GarmentAccessory, PricingReferenceItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        operations.Verify(x => x.ListAsync(It.IsAny<ISpecification<GarmentPartOperation, PricingReferenceItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        pricingParameters.Verify(x => x.ListAsync(It.IsAny<ISpecification<PricingParameter, PricingReferenceItem>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
