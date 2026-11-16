using ActivityLog.Application.Features.Entries.EventHandlers;
using ActivityLog.Application.Features.Entries.Queries.GetActivityEntries;
using ActivityLog.Application.Features.Entries.Queries.GetActivityFilters;
using ActivityLog.Contracts.Activity;

namespace UnitTests.ActivityLog;

/// <summary>ACT-001 R-09, SC-09: list newest first, filters combine, paging, filter choices.</summary>
public sealed class GetActivityEntriesTests
{
    private static readonly DateTimeOffset Morning = new(2026, 11, 16, 7, 0, 0, TimeSpan.Zero); // 09:00 Kyiv

    private static async Task<ActivityLogTestHarness> SeedAsync()
    {
        var harness = new ActivityLogTestHarness();
        harness.Names.Names[8] = "Адмін";
        ActivityRecorded[] activities =
        [
            ActivityLogTestHarness.Activity(at: Morning, subjectTitle: "КП №125"),
            ActivityLogTestHarness.Activity(at: Morning.AddHours(1), action: ActivityActions.Changed, subjectTitle: "КП №125"),
            ActivityLogTestHarness.Activity(actorId: 8, roles: ["Admin"], at: Morning.AddHours(2), subjectType: "production-order", subjectTitle: "ВЗ-17"),
            ActivityLogTestHarness.Activity(at: Morning.AddDays(-1), subjectTitle: "КП №99 Kovalchuk"),
        ];
        foreach (var activity in activities)
        {
            await using var db = harness.Open();
            await new RecordActivityOutboxHandler(db, harness.Names, harness.Clock)
                .HandleAsync(ActivityLogTestHarness.Payload(activity), CancellationToken.None);
        }

        return harness;
    }

    private static async Task<ActivityEntriesPageDto> ListAsync(ActivityLogTestHarness harness, GetActivityEntriesQuery query)
    {
        await using var db = harness.Open();
        var result = await new GetActivityEntriesHandler(db).Handle(query, CancellationToken.None);
        Assert.That(result.IsSuccess, Is.True);
        return result.Value;
    }

    [Test]
    public async Task Entries_come_newest_first_with_the_total()
    {
        using var harness = await SeedAsync();
        var page = await ListAsync(harness, new GetActivityEntriesQuery());
        Assert.That(page.Total, Is.EqualTo(4));
        Assert.That(page.Items.Select(x => x.SubjectTitle),
            Is.EqualTo(new[] { "ВЗ-17", "КП №125", "КП №125", "КП №99 Kovalchuk" }));
    }

    [Test]
    public async Task The_manager_role_filter_hides_the_admin()
    {
        using var harness = await SeedAsync();
        var page = await ListAsync(harness, new GetActivityEntriesQuery(Role: "Manager"));
        Assert.That(page.Items.Select(x => x.ActorName).Distinct(), Is.EqualTo(new[] { "Олена Менеджер" }));
        Assert.That(page.Total, Is.EqualTo(3));
    }

    [Test]
    public async Task Filters_combine()
    {
        using var harness = await SeedAsync();
        var page = await ListAsync(harness, new GetActivityEntriesQuery(ActorId: 7, SubjectType: "quote",
            Action: ActivityActions.Changed, From: new DateOnly(2026, 11, 16), To: new DateOnly(2026, 11, 16)));
        Assert.That(page.Items.Single().Action, Is.EqualTo(ActivityActions.Changed));
    }

    [Test]
    public async Task The_period_is_whole_kyiv_days()
    {
        using var harness = await SeedAsync();
        var yesterday = await ListAsync(harness, new GetActivityEntriesQuery(From: new DateOnly(2026, 11, 15), To: new DateOnly(2026, 11, 15)));
        Assert.That(yesterday.Items.Single().SubjectTitle, Is.EqualTo("КП №99 Kovalchuk"));
    }

    // SQLite lower() changes Latin letters only; PostgreSQL lower() also handles Cyrillic.
    [Test]
    public async Task The_search_matches_every_word_of_the_title_in_any_case()
    {
        using var harness = await SeedAsync();
        var page = await ListAsync(harness, new GetActivityEntriesQuery(Search: "KOVALCHUK 99"));
        Assert.That(page.Items.Single().SubjectTitle, Is.EqualTo("КП №99 Kovalchuk"));
    }

    [Test]
    public async Task Pages_split_the_list()
    {
        using var harness = await SeedAsync();
        var second = await ListAsync(harness, new GetActivityEntriesQuery(Page: 2, PageSize: 3));
        Assert.That(second.Items.Single().SubjectTitle, Is.EqualTo("КП №99 Kovalchuk"));
        Assert.That(second.Total, Is.EqualTo(4));
    }

    [Test]
    public async Task Filter_choices_are_the_people_and_types_found_in_the_journal()
    {
        using var harness = await SeedAsync();
        await using var db = harness.Open();
        var filters = (await new GetActivityFiltersHandler(db).Handle(new GetActivityFiltersQuery(), CancellationToken.None)).Value;
        Assert.That(filters.Actors.Select(x => (x.Id, x.Name)), Is.EquivalentTo(new[] { (7, "Олена Менеджер"), (8, "Адмін") }));
        Assert.That(filters.SubjectTypes.Select(x => x.Key), Is.EquivalentTo(new[] { "quote", "production-order" }));
        Assert.That(filters.Actions, Does.Contain(ActivityActions.Deleted));
    }

    [Test]
    public void The_validator_answers_in_ukrainian()
    {
        var validator = new GetActivityEntriesValidator();
        var result = validator.Validate(new GetActivityEntriesQuery(Page: 0, PageSize: 500, Action: "Hack",
            From: new DateOnly(2026, 11, 20), To: new DateOnly(2026, 11, 1)));
        Assert.That(result.Errors.Select(x => x.ErrorMessage), Is.EquivalentTo(new[]
        {
            "Номер сторінки має бути від 1.", "На сторінці може бути від 1 до 100 записів.", "Невідома дія.",
            "Початок періоду має бути не пізніше кінця.",
        }));
        Assert.That(validator.Validate(new GetActivityEntriesQuery()).IsValid, Is.True);
    }
}
