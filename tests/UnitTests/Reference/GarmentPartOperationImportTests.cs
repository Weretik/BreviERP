using Moq;
using Reference.Application.Contracts.Catalog;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.GarmentPartOperation.Import;
using Reference.Application.Features.GarmentPartOperation.Import.DTOs;
using Reference.Application.Features.Shared.SheetImport;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.GarmentPartOperations.ValueObjects;

namespace UnitTests.Reference;

/// <summary>Import of the Calculator sheet «Операції» (REF-003).</summary>
public sealed class GarmentPartOperationImportTests
{
    private static readonly DateOnly CheckedAt = new(2026, 8, 30);

    private static GarmentPartOperationImportRow Row(string code, string group, string name, decimal min, int row = 2,
        bool perProduct = false) =>
        new(row, code, group, "Рукав", "Прямий", name, min, perProduct, CheckedAt, true, "Джерело: «Расценки», рядок 5.");

    private static Mock<IReferenceRepository<T>> Repository<T>(List<T> stored, List<T> added, List<T> deleted)
        where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot
    {
        var repository = new Mock<IReferenceRepository<T>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        repository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<T>, CancellationToken>((rows, _) => added.AddRange(rows))
            .ReturnsAsync((IEnumerable<T> rows, CancellationToken _) => rows);
        repository.Setup(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<T>, CancellationToken>((rows, _) => deleted.AddRange(rows))
            .ReturnsAsync(0);
        return repository;
    }

    [Test]
    public async Task Import_updates_by_code_creates_groups_and_deletes_old_rows_unless_in_use()
    {
        var sleeve = GarmentPart.Create(GarmentPartId.From(1), "Рукав");
        var oldGroup = GarmentPart.Create(GarmentPartId.From(2), "Рукава (старе)");
        var usedGroup = GarmentPart.Create(GarmentPartId.From(3), "Спинка (старе)");
        var coded = GarmentPartOperation.Create(GarmentPartOperationId.From(5),
            new GarmentPartOperationDetails("OP-001", sleeve.Id, "Рукав", "Прямий", "Рукав куртки: прямий", 5m, false,
                null, true, null));
        var stale = GarmentPartOperation.Create(GarmentPartOperationId.From(6), oldGroup.Id, "Втачать рукав", 4m);
        var used = GarmentPartOperation.Create(GarmentPartOperationId.From(7), usedGroup.Id, "Стачать спинку", 3m);

        var addedOperations = new List<GarmentPartOperation>();
        var deletedOperations = new List<GarmentPartOperation>();
        var addedGroups = new List<GarmentPart>();
        var deletedGroups = new List<GarmentPart>();
        var operations = Repository([coded, stale, used], addedOperations, deletedOperations);
        var groups = Repository([sleeve, oldGroup, usedGroup], addedGroups, deletedGroups);
        var usage = new Mock<IProductUsageReader>();
        usage.Setup(x => x.IsGarmentPartOperationUsedAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new ImportGarmentPartOperationsCommandHandler(operations.Object, groups.Object, usage.Object);

        var result = await handler.Handle(new ImportGarmentPartOperationsCommand(
        [
            Row("OP-001", "Рукав", "Рукав куртки: прямий", 6m, 2),
            Row("op-155", "Інше", "Нестандартна операція", 0m, 3, perProduct: true),
        ], DryRun: false, DeleteMissing: true), CancellationToken.None);

        var value = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(value.Applied, Is.True);
            Assert.That((value.Created, value.Updated, value.Unchanged, value.Errors), Is.EqualTo((1, 1, 0, 0)));
            Assert.That(value.Rows[0].Changes, Is.EquivalentTo(new[] { "Норма часу", "Дата перевірки", "Примітки" }));
            Assert.That(coded.Min, Is.EqualTo(6m));
            Assert.That(value.NewGroups, Is.EqualTo(new[] { "Інше" }));
            var created = addedOperations.Single();
            Assert.That((created.Id, created.Code, created.MinutesPerProduct, created.Min),
                Is.EqualTo((GarmentPartOperationId.From(8), "OP-155", true, 0m)));
            Assert.That(created.GarmentPartId, Is.EqualTo(addedGroups.Single().Id));
            Assert.That(deletedOperations, Is.EqualTo(new[] { stale }));
            Assert.That(used.IsActive, Is.False);
            Assert.That(value.KeptInUse, Is.EqualTo(new[] { "7 Стачать спинку" }));
            Assert.That(deletedGroups, Is.EqualTo(new[] { oldGroup }));
            Assert.That(value.DeletedGroups, Is.EqualTo(new[] { "Рукава (старе)" }));
        });
    }

    [Test]
    public async Task Preview_and_rows_with_errors_change_nothing()
    {
        var sleeve = GarmentPart.Create(GarmentPartId.From(1), "Рукав");
        var stale = GarmentPartOperation.Create(GarmentPartOperationId.From(6), sleeve.Id, "Втачать рукав", 4m);
        var added = new List<GarmentPartOperation>();
        var deleted = new List<GarmentPartOperation>();
        var operations = Repository([stale], added, deleted);
        var groups = Repository([sleeve], [], []);
        var handler = new ImportGarmentPartOperationsCommandHandler(operations.Object, groups.Object,
            new Mock<IProductUsageReader>().Object);

        var preview = await handler.Handle(new ImportGarmentPartOperationsCommand(
            [Row("OP-001", "Рукав", "Рукав куртки: прямий", 6m)], DryRun: true, DeleteMissing: true),
            CancellationToken.None);
        var withError = await handler.Handle(new ImportGarmentPartOperationsCommand(
            [Row("OP-001", "Рукав", "Рукав куртки: прямий", 6m), Row("OP-002", "Рукав", "Без норми", 0m, 3)],
            DryRun: false, DeleteMissing: true), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(preview.Value.Applied, Is.False);
            Assert.That(preview.Value.Deleted, Is.EqualTo(new[] { "6 Втачать рукав" }));
            Assert.That(withError.Value.Applied, Is.False);
            Assert.That(withError.Value.Rows[1].Action, Is.EqualTo(ReferenceImportActions.Error));
            Assert.That(withError.Value.Rows[1].Error, Is.EqualTo("Немає норми часу."));
            Assert.That(added, Is.Empty);
            Assert.That(deleted, Is.Empty);
        });
        operations.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Operation_with_time_per_product_stores_no_norm_and_codes_are_normalized()
    {
        var operation = GarmentPartOperation.Create(GarmentPartOperationId.From(1),
            new GarmentPartOperationDetails(" op-155 ", GarmentPartId.From(1), null, null, "Нестандартна операція", 12m,
                true, null, true, null));

        Assert.Multiple(() =>
        {
            Assert.That(operation.Code, Is.EqualTo("OP-155"));
            Assert.That(operation.Min, Is.EqualTo(0m));
            Assert.That(operation.MinutesPerProduct, Is.True);
        });
    }
}
