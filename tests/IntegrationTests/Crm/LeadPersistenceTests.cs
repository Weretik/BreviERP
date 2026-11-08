using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.GetLeads;
using Crm.Application.Leads.GetMyDay;
using Crm.Application.Leads.RecordLeadCall;
using Crm.Application.Leads.ReleaseLead;
using Crm.Application.Leads.Sheet;
using Crm.Application.Leads.Stats;
using Crm.Application.Leads.TakeLead;
using Crm.Application.Tasks.Staff;
using Crm.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests.Crm;

/// <summary>Leads over the real crm schema: sync, taking, calls with «Подзвонити» tasks, «Мій день», stats (LEAD-001).</summary>
[NonParallelizable]
public sealed class LeadPersistenceTests : CrmDatabaseFixture
{
    private static readonly LeadActor Anastasia = new(17, false);
    private static readonly LeadActor Vadym = new(18, false);
    private static readonly LeadActor Director = new(1, true);

    private readonly Staff _staff = new();
    private readonly MovableClock _clock = new(new DateTimeOffset(2026, 11, 9, 7, 0, 0, TimeSpan.Zero));
    private readonly Sheet _sheet = new();

    private static readonly string?[] Header =
        ["ID", "Дата добавления", "Кто взял в работу", "Категория", "Предприятие", "Регион", "Отрасль", "Повод для звонка"];

    [SetUp]
    public void Reset()
    {
        _clock.Now = new DateTimeOffset(2026, 11, 9, 7, 0, 0, TimeSpan.Zero);
        _sheet.Rows =
        [
            Header,
            ["1", "2026-08-19", "Анастасия", "A", "L&S Distributions", "Львів", "Склад", "Новий склад"],
            ["2", "2026-08-20", null, "B", "ERC Київ", "Київ", "Склад", "Комплектувальники"],
            ["3", "2026-08-21", null, "A", "Suziria Group", "Калуш", "Харчове", "Новий завод"],
        ];
    }

    private async Task<LeadSyncResponse> Sync()
    {
        await using var db = Context();
        var result = await new LeadSheetSync(db, _sheet, _clock, NullLogger<LeadSheetSync>.Instance).RunAsync(default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        return result.Value;
    }

    private async Task<long> IdOf(int sourceId)
    {
        await using var db = Context();
        return await db.Leads.Where(x => x.SourceId == sourceId).Select(x => x.Id).SingleAsync();
    }

    private async Task<long> ResultId(string name)
    {
        await using var db = Context();
        return await db.LeadCallResults.Where(x => x.Name == name).Select(x => x.Id).SingleAsync();
    }

    private async Task<Ardalis.Result.Result<LeadDetailResponse>> Take(LeadActor actor, long id)
    {
        await using var db = Context();
        return await new TakeLeadCommandHandler(db, _staff, _clock).Handle(new TakeLeadCommand(actor, id), default);
    }

    private async Task<Ardalis.Result.Result<LeadDetailResponse>> Call(LeadActor actor, LeadDetailResponse lead,
        string result, DateTimeOffset? next = null, string? comment = null)
    {
        await using var db = Context();
        return await new RecordLeadCallCommandHandler(db, _staff, _clock).Handle(
            new RecordLeadCallCommand(actor, lead.Id, new RecordLeadCallInput(await ResultId(result), comment, next, lead.Version)),
            default);
    }

    [Test]
    public async Task SyncAddsUpdatesAndMarksGoneWithoutTouchingTheWork()
    {
        var first = await Sync();
        Assert.That((first.Rows, first.Added, first.Updated, first.Gone), Is.EqualTo((3, 3, 0, 0)));

        var taken = (await Take(Anastasia, await IdOf(1))).Value;
        _sheet.Rows = [Header, ["1", "2026-08-19", null, "A", "L&S Distributions LLC", "Львів", "Склад", "Новий склад"],
            ["2", "2026-08-20", null, "B", "ERC Київ", "Київ", "Склад", "Комплектувальники"]];
        var second = await Sync();

        await using var db = Context();
        var lead = await db.Leads.SingleAsync(x => x.SourceId == 1);
        Assert.Multiple(() =>
        {
            Assert.That((second.Added, second.Updated, second.Gone), Is.EqualTo((0, 1, 1)));
            Assert.That(lead.Company, Is.EqualTo("L&S Distributions LLC"));
            Assert.That(lead.OwnerUserId, Is.EqualTo(17));
            Assert.That(lead.SheetNotes, Is.EqualTo("Хто взяв: Анастасия"));
            Assert.That(taken.Stage, Is.EqualTo("InWork"));
            Assert.That(db.Leads.Single(x => x.SourceId == 3).InSheet, Is.False);
            Assert.That(db.LeadCategories.Select(x => x.Code).OrderBy(x => x).ToList(), Is.EqualTo(new[] { "A", "B" }));
            Assert.That(db.LeadSyncStates.Single().Rows, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task SecondManagerCannotTakeOrCallATakenLead()
    {
        await Sync();
        var id = await IdOf(1);
        var lead = (await Take(Anastasia, id)).Value;

        var take = await Take(Vadym, id);
        var call = await Call(Vadym, lead, "Не дозвонились");

        Assert.That(take.Status, Is.EqualTo(Ardalis.Result.ResultStatus.Conflict));
        Assert.That(take.Errors.Single(), Does.Contain("Анастасія"));
        Assert.That(call.Status, Is.EqualTo(Ardalis.Result.ResultStatus.Forbidden));
    }

    [Test]
    public async Task CallsKeepOneOpenCallTaskAndClosingEndsIt()
    {
        await Sync();
        var lead = (await Take(Anastasia, await IdOf(1))).Value;

        var noDate = await Call(Anastasia, lead, "Передзвонити");
        Assert.That(noDate.Status, Is.EqualTo(Ardalis.Result.ResultStatus.Invalid));

        lead = (await Call(Anastasia, lead, "Не дозвонились")).Value;
        Assert.That(lead.NextContactAt, Is.EqualTo(_clock.Now.AddDays(1)));
        var firstTask = lead.OpenTaskId;

        lead = (await Call(Anastasia, lead, "Передзвонити", _clock.Now.AddDays(3), "Після обіду")).Value;
        await using (var db = Context())
        {
            var tasks = await db.ManagerTasks.OrderBy(x => x.Id).ToListAsync();
            Assert.That(tasks.Select(x => x.Status), Is.EqualTo(new[] { ManagerTaskStatus.Done, ManagerTaskStatus.New }));
            Assert.That(tasks[1].Title, Is.EqualTo("Подзвонити: L&S Distributions"));
            Assert.That(tasks[1].AssigneeUserId, Is.EqualTo(17));
            Assert.That(tasks[1].DueAt, Is.EqualTo(_clock.Now.AddDays(3)));
            Assert.That(tasks[1].LeadId, Is.EqualTo(lead.Id));
            Assert.That(lead.OpenTaskId, Is.EqualTo(tasks[1].Id).And.Not.EqualTo(firstTask));
        }

        lead = (await Call(Anastasia, lead, "Відмова", _clock.Now.AddDays(5))).Value;
        await using (var db = Context())
        {
            Assert.That(await db.ManagerTasks.CountAsync(x => x.Status != ManagerTaskStatus.Done), Is.Zero);
        }

        Assert.Multiple(() =>
        {
            Assert.That(lead.Stage, Is.EqualTo("Closed"));
            Assert.That(lead.NextContactAt, Is.Null);
            Assert.That(lead.CallCount, Is.EqualTo(3));
            Assert.That(lead.Calls.First().ResultName, Is.EqualTo("Відмова"));
        });
    }

    [Test]
    public async Task ReleaseDeletesTheOpenTaskAndOnlyOwnerOrDirectorMayDoIt()
    {
        await Sync();
        var lead = (await Take(Anastasia, await IdOf(2))).Value;
        lead = (await Call(Anastasia, lead, "Передзвонити", _clock.Now.AddDays(1))).Value;

        await using (var db = Context())
        {
            var byVadym = await new ReleaseLeadCommandHandler(db, _clock)
                .Handle(new ReleaseLeadCommand(Vadym, lead.Id, lead.Version), default);
            Assert.That(byVadym.Status, Is.EqualTo(Ardalis.Result.ResultStatus.Forbidden));
        }

        await using (var db = Context())
        {
            var byDirector = await new ReleaseLeadCommandHandler(db, _clock)
                .Handle(new ReleaseLeadCommand(Director, lead.Id, lead.Version), default);
            Assert.That(byDirector.Value.Owner, Is.Null);
            Assert.That(byDirector.Value.Stage, Is.EqualTo("InWork"));
            Assert.That(await db.ManagerTasks.CountAsync(), Is.Zero);
        }
    }

    [Test]
    public async Task MyDayListsOwnDueAndNewLeadsAndListsCountChips()
    {
        await Sync();
        var due = (await Take(Anastasia, await IdOf(1))).Value;
        await Call(Anastasia, due, "Передзвонити", _clock.Now.AddHours(2));
        var later = (await Take(Anastasia, await IdOf(2))).Value;
        await Call(Anastasia, later, "Передзвонити", _clock.Now.AddDays(4));

        await using var db = Context();
        var day = (await new GetMyDayQueryHandler(db, _clock).Handle(new GetMyDayQuery(Anastasia), default)).Value;
        var page = (await new GetLeadsQueryHandler(db, _clock)
            .Handle(new GetLeadsQuery(Anastasia, "київ", "All", null, null, 1, 25), default)).Value;
        var free = (await new GetLeadsQueryHandler(db, _clock)
            .Handle(new GetLeadsQuery(Vadym, null, "Free", null, "Open", 1, 25), default)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(day.Queue.Select(x => x.SourceId), Is.EqualTo(new[] { 1 }));
            Assert.That(day.Later.Select(x => x.SourceId), Is.EqualTo(new[] { 2 }));
            Assert.That((day.CallsToday, day.PlannedToday, day.Free), Is.EqualTo((2, 3, 1)));
            Assert.That(page.Items.Select(x => x.SourceId), Is.EqualTo(new[] { 2 }));
            Assert.That((page.All, page.Mine, page.Free), Is.EqualTo((1, 1, 0)));
            Assert.That(free.Items.Select(x => x.SourceId), Is.EqualTo(new[] { 3 }));
        });
    }

    [Test]
    public async Task StatsCountManagersResultsAndIndustries()
    {
        await Sync();
        var lead = (await Take(Anastasia, await IdOf(1))).Value;
        lead = (await Call(Anastasia, lead, "Не дозвонились")).Value;
        await Call(Anastasia, lead, "Надіслали пропозицію", _clock.Now.AddDays(2));

        await using var db = Context();
        var stats = (await new GetLeadStatsQueryHandler(db, _clock).Handle(new GetLeadStatsQuery(null, null), default)).Value;

        Assert.Multiple(() =>
        {
            Assert.That((stats.Total, stats.Free, stats.Offer, stats.CallsInPeriod), Is.EqualTo((3, 2, 1, 2)));
            var row = stats.Managers.Single();
            Assert.That((row.Manager.Name, row.Calls, row.ReachedPercent, row.Offers), Is.EqualTo(("Анастасія", 2, 50, 1)));
            Assert.That(stats.Results.Select(x => x.Name), Is.EquivalentTo(new[] { "Не дозвонились", "Надіслали пропозицію" }));
            Assert.That(stats.Industries.First().Industry, Is.EqualTo("Склад"));
            Assert.That(stats.Sync!.Rows, Is.EqualTo(3));
        });
    }

    private sealed class Sheet : ILeadSheetReader
    {
        public IReadOnlyList<IReadOnlyList<string?>> Rows { get; set; } = [];

        public Task<IReadOnlyList<IReadOnlyList<string?>>> ReadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Rows);
    }

    private sealed class Staff : ITaskStaffReader
    {
        private static readonly TaskStaffMember[] People =
            [new(1, "Директор", null), new(17, "Анастасія", null), new(18, "Вадим", null)];

        public Task<IReadOnlyList<TaskStaffMember>> ListAssigneesAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TaskStaffMember>>(People);

        public Task<TaskStaffMember?> FindAssigneeAsync(int userId, CancellationToken cancellationToken) =>
            Task.FromResult(People.FirstOrDefault(x => x.UserId == userId));

        public Task<TaskStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
            Task.FromResult(People.FirstOrDefault(x => x.UserId == userId));
    }

    private sealed class MovableClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
