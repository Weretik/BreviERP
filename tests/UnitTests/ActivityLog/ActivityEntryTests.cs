using ActivityLog.Domain.Entries;

namespace UnitTests.ActivityLog;

/// <summary>ACT-001 R-02: limits of an entry.</summary>
public sealed class ActivityEntryTests
{
    [Test]
    public void Long_texts_are_cut_with_an_ellipsis_and_changes_kept_to_twenty()
    {
        var changes = Enumerable.Range(1, 25).Select(i => ActivityChange.Of($"Поле {i}", null, new string('x', 300)));
        var entry = ActivityEntry.Create(Guid.CreateVersion7(), DateTimeOffset.UtcNow, 1, "Ім'я", ["Manager", "Admin", "Manager"],
            "Changed", "quote", "КП", "1", new string('К', 250), new string('n', 600), changes, "Crm", DateTimeOffset.UtcNow);

        Assert.That(entry.SubjectTitle, Has.Length.EqualTo(ActivityEntry.SubjectTitleMaxLength).And.EndsWith("…"));
        Assert.That(entry.Summary, Has.Length.EqualTo(ActivityEntry.SummaryMaxLength));
        Assert.That(entry.Changes, Has.Count.EqualTo(ActivityEntry.MaxChanges));
        Assert.That(entry.Changes[0].To, Has.Length.EqualTo(ActivityChange.ValueMaxLength));
        Assert.That(entry.ActorRoles, Is.EqualTo("|Admin|Manager|"));
        Assert.That(entry.RoleNames, Is.EqualTo(new[] { "Admin", "Manager" }));
    }

    [Test]
    public void An_entry_needs_its_event_id() =>
        Assert.That(() => ActivityEntry.Create(Guid.Empty, DateTimeOffset.UtcNow, 1, "x", [], "Created", "quote", "КП",
            "1", "№1", null, [], "Crm", DateTimeOffset.UtcNow), Throws.ArgumentException);
}
