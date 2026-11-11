using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Application.Stock.Movements;
using Warehouse.Application.Stock.OneOff;
using Warehouse.Application.Stock.Revisions;
using static UnitTests.Warehouse.WarehouseStockHandlerTests;

namespace UnitTests.Warehouse;

/// <summary>Edit, void and restore of movements with «Історія правок» (WH-003).</summary>
public sealed class WarehouseRevisionTests
{
    private static readonly WarehouseActor Chief = new(4, false, false, true);
    private static readonly WarehouseActor Manager = new(2, false, true, false);
    private static readonly DateOnly Day = new(2026, 11, 4);

    [Test]
    public async Task Editing_a_receipt_restates_the_chain_and_keeps_before_and_after()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        await Receive(workshop, Chief, "Receipt", 20m, supplier: "Тексика");
        await IssueOneOff(workshop, 5m);
        var receipt = await Row(workshop, "Receipt");

        var edited = await Edit(workshop, Chief, receipt, 25m, supplier: "Черкаси ВВ", reason: "Перерахували рулон");
        var stale = await Edit(workshop, Chief, receipt, 30m);
        var rows = (await Movements(workshop, Chief, FabricMovementFilter.All)).Rows;
        var history = await History(workshop, receipt.Id);

        Assert.Multiple(() =>
        {
            Assert.That(edited.Value.Meters, Is.EqualTo(30m));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict), "revision 0 was changed");
            Assert.That(rows.Single(x => x.Kind == "Issue").MetersAfter, Is.EqualTo(30m), "later movements restated");
            Assert.That(rows.Single(x => x.Kind == "Receipt"),
                Has.Property("SupplierName").EqualTo("Черкаси ВВ").And.Property("Revision").EqualTo(1));
            Assert.That(history.Revisions.Single(), Has.Property("Action").EqualTo("Edit")
                .And.Property("Reason").EqualTo("Перерахували рулон"));
            Assert.That(history.Revisions[0].Before.Delta, Is.EqualTo(20m));
            Assert.That(history.Revisions[0].After.Delta, Is.EqualTo(25m));
            Assert.That(history.CanRestore, Is.True);
        });
    }

    [Test]
    public async Task A_receipt_moved_to_another_colour_restates_both_lines()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Receipt", 12m);
        var receipt = await Row(workshop, "Receipt");

        var moved = await Edit(workshop, Chief, receipt, 12m, fabricId: 23, color: "Червоний");
        var list = await List(workshop, Chief);
        var restored = await Restore(workshop, receipt.Id, 1);
        var back = await List(workshop, Chief);

        Assert.Multiple(() =>
        {
            Assert.That(moved.Value, Has.Property("Color").EqualTo("Червоний").And.Property("Meters").EqualTo(12m));
            Assert.That(Pair(list, "Темно-сірий").Meters, Is.EqualTo(0m));
            Assert.That(Pair(list, "Червоний").Meters, Is.EqualTo(12m));
            Assert.That(restored.IsSuccess, Is.True);
            Assert.That(Pair(back, "Темно-сірий").Meters, Is.EqualTo(12m));
            Assert.That(Pair(back, "Червоний").Meters, Is.EqualTo(0m));
        });
    }

    [Test]
    public async Task A_back_dated_edit_moves_the_movement_in_the_chain_and_an_issue_may_change_pair()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m, date: Day.AddDays(-5));
        await Receive(workshop, Chief, "Opening", 4m, color: "Червоний", date: Day.AddDays(-5));
        await IssueOneOff(workshop, 5m);
        await Receive(workshop, Chief, "Receipt", 3m, date: Day.AddDays(1));
        var receipt = await Row(workshop, "Receipt");
        var issue = await Row(workshop, "Issue");

        await Edit(workshop, Chief, receipt, 3m, date: Day.AddDays(-1));
        var rows = (await Movements(workshop, Chief, FabricMovementFilter.All, color: "Темно-сірий")).Rows;
        var moved = await Edit(workshop, Chief, issue, 5m, fabricCode: "FAB-023", color: "Червоний");
        var list = await List(workshop, Chief);

        Assert.Multiple(() =>
        {
            Assert.That(rows.Single(x => x.Kind == "Receipt").MetersAfter, Is.EqualTo(13m), "A29: now before the issue");
            Assert.That(rows.Single(x => x.Kind == "Issue").MetersAfter, Is.EqualTo(8m));
            Assert.That(moved.Value.Color, Is.EqualTo("Червоний"));
            Assert.That(Pair(list, "Темно-сірий").Meters, Is.EqualTo(13m), "A30: the old pair gets the 5 m back");
            Assert.That(Pair(list, "Червоний").Meters, Is.EqualTo(-1m), "A30: the new pair spends it");
        });
    }

    [Test]
    public async Task Void_keeps_the_row_without_effect_and_restore_brings_it_back()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        await Receive(workshop, Chief, "Receipt", 20m);
        var receipt = await Row(workshop, "Receipt");

        var shortReason = await Void(workshop, receipt.Id, 0, "ой");
        var voided = await Void(workshop, receipt.Id, 0, "Дубль накладної");
        var row = await Row(workshop, "Receipt");
        var reverse = await Reverse(workshop, Chief, receipt.Id, "Сторно скасованого");
        var restored = await Restore(workshop, receipt.Id, 1);
        var again = await Restore(workshop, receipt.Id, 2);
        var history = await History(workshop, receipt.Id);

        Assert.Multiple(() =>
        {
            Assert.That(shortReason.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(voided.Value.Meters, Is.EqualTo(10m));
            Assert.That(row, Has.Property("IsVoided").EqualTo(true).And.Property("Delta").EqualTo(0m)
                .And.Property("CanReverse").EqualTo(false));
            Assert.That(reverse.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(restored.Value.Meters, Is.EqualTo(30m));
            Assert.That(again.Status, Is.EqualTo(ResultStatus.Conflict), "nothing left to restore");
            Assert.That(history.Revisions.Select(x => x.Action), Is.EqualTo(new[] { "Restore", "Void" }));
            Assert.That(history.Revisions[1].IsUndone, Is.True);
            Assert.That(history.CanRestore, Is.False);
        });
    }

    [Test]
    public async Task Restores_go_back_one_change_at_a_time()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Receipt", 10m);
        var receipt = await Row(workshop, "Receipt");
        await Edit(workshop, Chief, receipt, 15m);
        await Edit(workshop, Chief, receipt with { Revision = 1 }, 18m);

        var first = await Restore(workshop, receipt.Id, 2);
        var second = await Restore(workshop, receipt.Id, 3);

        Assert.Multiple(() =>
        {
            Assert.That(first.Value.Meters, Is.EqualTo(15m));
            Assert.That(second.Value.Meters, Is.EqualTo(10m));
        });
    }

    [Test]
    public async Task Edits_check_the_sign_the_kind_and_who_may_move_stock()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Opening", 10m);
        await IssueOneOff(workshop, 4m);
        var issue = await Row(workshop, "Issue");
        var opening = await Row(workshop, "Opening");
        var id = Guid.NewGuid();

        var manager = await Edit(workshop, Manager, issue, 3m);
        var negative = await Edit(workshop, Chief, issue, -3m);
        var openingTarget = await Edit(workshop, Chief, opening, 10m, fabricId: 5);
        var unchanged = await Edit(workshop, Chief, opening, 10m);
        var issueEdit = await Edit(workshop, Chief, issue, 3m, destination: "Зразки", operationId: id);
        var repeat = await Edit(workshop, Chief, issue, 3m, destination: "Зразки", operationId: id);
        var row = await Row(workshop, "Issue");

        Assert.Multiple(() =>
        {
            Assert.That(manager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(negative.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(openingTarget.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(unchanged.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(issueEdit.Value.Meters, Is.EqualTo(7m));
            Assert.That(repeat.Value.Replayed, Is.True);
            Assert.That(row, Has.Property("Delta").EqualTo(-3m).And.Property("Destination").EqualTo("Зразки"));
        });
    }

    [Test]
    public async Task A_reversal_and_a_reversed_movement_cannot_be_edited()
    {
        using var workshop = Workshop();
        await Receive(workshop, Chief, "Receipt", 10m);
        var receipt = await Row(workshop, "Receipt");
        await Reverse(workshop, Chief, receipt.Id, "Помилка");
        var rows = (await Movements(workshop, Chief, FabricMovementFilter.All)).Rows;
        var reversal = rows.Single(x => x.Kind == "Reversal");

        var editReversed = await Edit(workshop, Chief, receipt, 5m);
        var voidReversal = await Void(workshop, reversal.Id, 0, "Скасувати сторно");

        Assert.Multiple(() =>
        {
            Assert.That(rows.Select(x => x.CanEdit), Is.All.False);
            Assert.That(editReversed.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(voidReversal.Status, Is.EqualTo(ResultStatus.Conflict));
        });
    }

    private static async Task<FabricMovementRowResponse> Row(WarehouseTestHarness workshop, string kind)
    {
        workshop.Db.ChangeTracker.Clear();
        return (await Movements(workshop, Chief, FabricMovementFilter.All)).Rows.Single(x => x.Kind == kind);
    }

    private static Task<Result<FabricStockOperationResponse>> IssueOneOff(WarehouseTestHarness workshop,
        decimal meters) =>
        new IssueFabricCommandHandler(workshop.StockChanges(), workshop.Pairs(), workshop.Operations())
            .Handle(new(Chief, new(Guid.NewGuid(), "FAB-023", "Темно-сірий", null, meters, Day, "Цех", null)),
                CancellationToken.None).AsTask();

    private static MovementRevisions Revisions(WarehouseTestHarness workshop) =>
        new(workshop.Db, workshop.Operations(), workshop.Pairs());

    private static async Task<Result<FabricStockOperationResponse>> Edit(WarehouseTestHarness workshop,
        WarehouseActor actor, FabricMovementRowResponse row, decimal meters, int? fabricId = null,
        string? color = null, string? supplier = null, string? destination = null, string? reason = null,
        Guid? operationId = null, string? fabricCode = null, DateOnly? date = null)
    {
        workshop.Db.ChangeTracker.Clear();
        return await new EditFabricMovementCommandHandler(workshop.StockChanges(), workshop.Pairs(),
                workshop.Operations(), Revisions(workshop), workshop.Colors())
            .Handle(new(actor, row.Id, new(operationId ?? Guid.NewGuid(), row.Revision, date ?? row.OperationDate, meters,
                fabricId, fabricCode, color, supplier ?? row.SupplierName, row.Document, destination ?? row.Destination,
                reason)), CancellationToken.None);
    }

    private static async Task<Result<FabricStockOperationResponse>> Void(WarehouseTestHarness workshop, int id,
        int revision, string reason)
    {
        workshop.Db.ChangeTracker.Clear();
        var command = new VoidFabricMovementCommand(Chief, id, new(Guid.NewGuid(), revision, reason));
        var errors = new VoidFabricMovementCommandValidator().Validate(command);
        if (!errors.IsValid) return Result<FabricStockOperationResponse>.Invalid(new ValidationError("Reason", "short"));
        return await new VoidFabricMovementCommandHandler(workshop.StockChanges(), workshop.Pairs(),
            workshop.Operations(), Revisions(workshop)).Handle(command, CancellationToken.None);
    }

    private static async Task<Result<FabricStockOperationResponse>> Restore(WarehouseTestHarness workshop, int id,
        int revision)
    {
        workshop.Db.ChangeTracker.Clear();
        return await new RestoreFabricMovementCommandHandler(workshop.Db, workshop.StockChanges(), workshop.Pairs(),
                workshop.Operations(), Revisions(workshop))
            .Handle(new(Chief, id, new(Guid.NewGuid(), revision)), CancellationToken.None);
    }

    private static async Task<FabricMovementHistoryResponse> History(WarehouseTestHarness workshop, int id)
    {
        workshop.Db.ChangeTracker.Clear();
        return (await new GetFabricMovementHistoryQueryHandler(workshop.Db)
            .Handle(new(Chief, id), CancellationToken.None)).Value;
    }
}
