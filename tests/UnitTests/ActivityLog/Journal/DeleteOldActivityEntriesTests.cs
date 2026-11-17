using ActivityLog.Application.Features.Entries.Commands.DeleteOldActivityEntries;
using ActivityLog.Application.Features.Entries.EventHandlers;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.ActivityLog.Journal;

/// <summary>ACT-001 R-12 (owner 2026-11-17): «Журнал дій» keeps entries for one year; older ones are deleted daily.</summary>
public sealed class DeleteOldActivityEntriesTests
{
    private static async Task RecordAsync(ActivityLogTestHarness harness, DateTimeOffset at, string title)
    {
        await using var db = harness.Open();
        await new RecordActivityOutboxHandler(db, harness.Names, harness.Clock).HandleAsync(
            ActivityLogTestHarness.Payload(ActivityLogTestHarness.Activity(at: at, subjectTitle: title)),
            CancellationToken.None);
    }

    private static async Task<int> DeleteAsync(ActivityLogTestHarness harness)
    {
        await using var db = harness.Open();
        var result = await new DeleteOldActivityEntriesHandler(db, harness.Clock)
            .Handle(new DeleteOldActivityEntriesCommand(), CancellationToken.None);
        Assert.That(result.IsSuccess, Is.True);
        return result.Value;
    }

    [Test]
    public async Task Entries_older_than_a_year_are_deleted_and_the_rest_stay()
    {
        using var harness = new ActivityLogTestHarness();
        var now = harness.Clock.GetUtcNow();
        await RecordAsync(harness, now.AddYears(-1).AddMinutes(-1), "КП №1");
        await RecordAsync(harness, now.AddYears(-1).AddMinutes(1), "КП №2");
        await RecordAsync(harness, now.AddDays(-1), "КП №3");

        var deleted = await DeleteAsync(harness);

        Assert.That(deleted, Is.EqualTo(1));
        await using var db = harness.Open();
        Assert.That(await db.Entries.Select(x => x.SubjectTitle).OrderBy(x => x).ToListAsync(),
            Is.EqualTo(new[] { "КП №2", "КП №3" }));
    }

    [Test]
    public async Task A_repeated_run_deletes_nothing()
    {
        using var harness = new ActivityLogTestHarness();
        await RecordAsync(harness, harness.Clock.GetUtcNow().AddYears(-2), "КП №1");

        Assert.That(await DeleteAsync(harness), Is.EqualTo(1));
        Assert.That(await DeleteAsync(harness), Is.EqualTo(0));
    }
}
