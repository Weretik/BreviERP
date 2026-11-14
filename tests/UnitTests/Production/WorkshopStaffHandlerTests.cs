using Ardalis.Result;
using Production.Application.Features.Staff.Commands.CreateWorkshopEmployee;
using Production.Application.Features.Staff.Commands.ImportWorkshopStaff;
using Production.Application.Features.Staff.Commands.SetWorkshopEmployeeActive;
using Production.Application.Features.Staff.Commands.UpdateWorkshopEmployee;
using Production.Application.Features.Staff.Queries.GetWorkshopStaff;
using Production.Application.Features.Staff.Shared;

namespace UnitTests.Production;

public sealed class WorkshopStaffHandlerTests
{
    [Test]
    public async Task Active_worker_names_are_unique_ignoring_case_and_spaces()
    {
        using var workshop = new ProductionTestHarness();
        var create = new CreateWorkshopEmployeeHandler(workshop.Db, workshop.Time);

        var first = await create.Handle(new(new WorkshopEmployeeInput("Олена  Коваль", "Швачка"), 7), CancellationToken.None);
        var duplicate = await create.Handle(new(new WorkshopEmployeeInput(" олена коваль ", "Закрійниця"), 7), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(first.Value.FullName, Is.EqualTo("Олена Коваль"));
            Assert.That(duplicate.Status, Is.EqualTo(ResultStatus.Conflict));
        });
    }

    [Test]
    public async Task Archived_worker_leaves_the_list_and_frees_the_name()
    {
        using var workshop = new ProductionTestHarness();
        var create = new CreateWorkshopEmployeeHandler(workshop.Db, workshop.Time);
        var worker = (await create.Handle(new(new WorkshopEmployeeInput("Олена Коваль", "Швачка"), 7), CancellationToken.None)).Value;

        await new SetWorkshopEmployeeActiveHandler(workshop.Db, workshop.Time)
            .Handle(new(worker.Id, false, 7), CancellationToken.None);
        var again = await create.Handle(new(new WorkshopEmployeeInput("Олена Коваль", "Швачка"), 7), CancellationToken.None);
        var list = new GetWorkshopStaffHandler(workshop.Db);
        var active = await list.Handle(new GetWorkshopStaffQuery(false), CancellationToken.None);
        var all = await list.Handle(new GetWorkshopStaffQuery(true), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(again.IsSuccess, Is.True);
            Assert.That(active.Value.Select(x => x.Id), Is.EqualTo(new[] { again.Value.Id }));
            Assert.That(all.Value, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task Rename_to_another_active_name_is_a_conflict()
    {
        using var workshop = new ProductionTestHarness();
        var create = new CreateWorkshopEmployeeHandler(workshop.Db, workshop.Time);
        await create.Handle(new(new WorkshopEmployeeInput("Олена Коваль", "Швачка"), 7), CancellationToken.None);
        var other = (await create.Handle(new(new WorkshopEmployeeInput("Ірина Бондар", "Швачка"), 7), CancellationToken.None)).Value;

        var result = await new UpdateWorkshopEmployeeHandler(workshop.Db, workshop.Time)
            .Handle(new(other.Id, new WorkshopEmployeeInput("ОЛЕНА КОВАЛЬ", "Швачка"), 7), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Import_preview_saves_nothing_and_apply_adds_new_and_updates_positions()
    {
        using var workshop = new ProductionTestHarness();
        await new CreateWorkshopEmployeeHandler(workshop.Db, workshop.Time)
            .Handle(new(new WorkshopEmployeeInput("Олена Коваль", "Швачка"), 7), CancellationToken.None);
        var handler = new ImportWorkshopStaffHandler(workshop.Db, workshop.Time);
        WorkshopEmployeeImportRow[] rows = [new("олена коваль", "Майстер цеху"), new("Ірина Бондар", "Швачка")];

        var preview = await handler.Handle(new(rows, true, 7), CancellationToken.None);
        var countAfterPreview = workshop.Db.Employees.Count();
        var applied = await handler.Handle(new(rows, false, 7), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That((preview.Value.Created, preview.Value.Updated), Is.EqualTo((1, 1)));
            Assert.That(countAfterPreview, Is.EqualTo(1));
            Assert.That(applied.Value.Lines.Select(x => x.Action), Is.EqualTo(new[] { "updated", "created" }));
            Assert.That(workshop.Db.Employees.Single(x => x.FullName == "Олена Коваль").Position, Is.EqualTo("Майстер цеху"));
        });
    }

    [Test]
    public async Task Import_validator_rejects_repeated_names()
    {
        var result = await new ImportWorkshopStaffValidator().ValidateAsync(
            new ImportWorkshopStaffCommand([new("Олена Коваль", "Швачка"), new("ОЛЕНА  коваль", "Швачка")], true, 7));

        Assert.That(result.IsValid, Is.False);
    }
}
