using Ardalis.Result;
using Ardalis.Specification;
using BuildingBlocks.Domain.ValueObjects;
using Moq;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Reference.Application.Abstractions;
using Reference.Application.Features.Fabric.Commands.DeleteFabric;
using Reference.Application.Features.GarmentAccessory.Commands.DeleteGarmentAccessory;
using Reference.Application.Features.GarmentPartOperation.Commands.DeleteGarmentPartOperation;
using Reference.Domain.Fabrics;
using Reference.Domain.GarmentParts;

namespace UnitTests.Reference;

public sealed class ReferenceProductUsageTests
{
    [Test]
    public async Task Deleting_reference_data_used_by_product_is_rejected_without_delete()
    {
        var usage = new Mock<IProductUsageReader>();
        usage.Setup(x => x.IsFabricUsedAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        usage.Setup(x => x.IsGarmentAccessoryUsedAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        usage.Setup(x => x.IsGarmentPartOperationUsedAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var fabrics = Repository(Fabric.Create(FabricId.From(1), "Тканина", MoneyAmount.From(1m), SupplierId.From(1)).Value);
        var accessories = Repository(GarmentAccessory.Create(GarmentAccessoryId.From(2), "Фурнітура", MoneyAmount.From(1m)).Value);
        var operations = Repository(GarmentPartOperation.Create(GarmentPartOperationId.From(3), GarmentPartId.From(1), "Операція", 1m).Value);

        var fabricResult = await new DeleteFabricHandler(fabrics.Object, usage.Object).Handle(new DeleteFabricCommand(1), CancellationToken.None);
        var accessoryResult = await new DeleteGarmentAccessoryHandler(accessories.Object, usage.Object).Handle(new DeleteGarmentAccessoryCommand(2), CancellationToken.None);
        var operationResult = await new DeleteGarmentPartOperationHandler(operations.Object, usage.Object).Handle(new DeleteGarmentPartOperationCommand(3), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(fabricResult.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(accessoryResult.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(operationResult.Status, Is.EqualTo(ResultStatus.Conflict));
        });
        fabrics.Verify(x => x.DeleteAsync(It.IsAny<Fabric>(), It.IsAny<CancellationToken>()), Times.Never);
        accessories.Verify(x => x.DeleteAsync(It.IsAny<GarmentAccessory>(), It.IsAny<CancellationToken>()), Times.Never);
        operations.Verify(x => x.DeleteAsync(It.IsAny<GarmentPartOperation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<IReferenceRepository<T>> Repository<T>(T entity) where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot
    {
        var repository = new Mock<IReferenceRepository<T>>();
        repository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<T>>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return repository;
    }
}
