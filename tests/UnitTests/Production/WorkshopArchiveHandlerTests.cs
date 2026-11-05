using Ardalis.Result;
using Production.Application.Archive.GetArchive;
using Production.Application.Archive.ImportArchive;
using Production.Application.Contracts;
using Production.Application.Orders.Access;

namespace UnitTests.Production;

/// <summary>«Архів цеху»: moving the old books once and reading them (PRD-008).</summary>
public sealed class WorkshopArchiveHandlerTests
{
    private static readonly ProductionActor Admin = new(1, true, false, false);
    private static readonly ProductionActor Manager = new(2, false, true, false);

    [Test]
    public async Task The_old_books_move_once_and_are_found_by_word_and_date()
    {
        using var workshop = ProductionOrderHandlerTests.Workshop();
        WorkshopArchiveImportRow[] rows =
        [
            Row("Schedule", "schedule:3", new DateOnly(2017, 2, 23), "Диканське", "грета оранжевая",
                "https://docs.google.com/document/d/abc"),
            Row("Schedule", "schedule:4", new DateOnly(2023, 3, 27), "Инверт 2023", "габардин", null),
            Row("Request", "request:a1", new DateOnly(2026, 2, 16), "Диканське, Околиця", "Адванта", null)
        ];
        var import = new ImportWorkshopArchiveCommandHandler(workshop.Db);

        var byManager = await import.Handle(new(Manager, rows, true), CancellationToken.None);
        var badLink = await import.Handle(new(Admin,
            [Row("Schedule", "schedule:9", null, "Х", null, "http://drive.google.com/x")], false), CancellationToken.None);
        var preview = await import.Handle(new(Admin, rows, true), CancellationToken.None);
        var applied = await import.Handle(new(Admin, rows, false), CancellationToken.None);
        var repeat = await import.Handle(new(Admin, rows, false), CancellationToken.None);
        var read = new GetWorkshopArchiveQueryHandler(workshop.Db);
        var schedule = await read.Handle(new(Manager, "Schedule", null, null, null, 1, 50), CancellationToken.None);
        var found = await read.Handle(new(Manager, "schedule", "ДИКАНСЬКЕ грета", null, null, 1, 50), CancellationToken.None);
        var dated = await read.Handle(new(Manager, "Schedule", null, new DateOnly(2020, 1, 1), null, 1, 50),
            CancellationToken.None);
        var invalid = new GetWorkshopArchiveQueryValidator()
            .Validate(new GetWorkshopArchiveQuery(Admin, "Other", null, new DateOnly(2020, 2, 1), new DateOnly(2020, 1, 1), 1, 500));

        Assert.Multiple(() =>
        {
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(badLink.Value.Errors, Is.EqualTo(1));
            Assert.That(badLink.Value.DryRun, Is.True, "an error saves nothing");
            Assert.That(preview.Value.Created, Is.EqualTo(3));
            Assert.That(applied.Value.DryRun, Is.False);
            Assert.That(repeat.Value.Unchanged, Is.EqualTo(3), "a repeat adds nothing");
            Assert.That(schedule.Value.Total, Is.EqualTo(2));
            Assert.That(schedule.Value.Items[0].Client, Is.EqualTo("Инверт 2023"), "newest first");
            Assert.That(found.Value.Items.Single().Links.Single().Label, Is.EqualTo("Документ"));
            Assert.That(dated.Value.Items.Single().SourceRow, Is.EqualTo(4));
            Assert.That(invalid.Errors.Select(x => x.PropertyName),
                Is.SupersetOf(new[] { "Kind", "From", "PageSize" }));
        });
    }

    private static WorkshopArchiveImportRow Row(string kind, string key, DateOnly? date, string client, string? fabric,
        string? link) => new(kind, key, int.TryParse(key.Split(':')[1], out var row) ? row : 7, date, null, null, client, "Костюм", "10+4", fabric, null, null, [new("Сезон", "Літо")],
        link is null ? [] : [new("Документ", link)]);
}
