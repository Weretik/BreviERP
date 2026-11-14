using Crm.Domain.Leads;
using Crm.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Crm.Application.Features.Leads.Commands.RecordLeadCall;
using Crm.Application.Features.Leads.Commands.ReleaseLead;
using Crm.Application.Features.Leads.Commands.TakeLead;
using Crm.Application.Features.Leads.Queries.GetLeadStats;
using Crm.Application.Features.Leads.Queries.GetLeads;
using Crm.Application.Features.Leads.Queries.GetMyDay;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Sheet;
using Crm.Application.Abstractions.Leads;
using Crm.Application.Abstractions.Tasks;

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
        var result = await new LeadSheetSync(db, _sheet, _staff, _clock, NullLogger<LeadSheetSync>.Instance).RunAsync(default);
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
        return await new TakeLeadHandler(db, _staff, _clock).Handle(new TakeLeadCommand(actor, id), default);
    }

    private async Task<Ardalis.Result.Result<LeadDetailResponse>> Call(LeadActor actor, LeadDetailResponse lead,
        string result, DateTimeOffset? next = null, string? comment = null)
    {
        await using var db = Context();
        return await new RecordLeadCallHandler(db, _staff, _clock).Handle(
            new RecordLeadCallCommand(actor, lead.Id, new RecordLeadCallInput(await ResultId(result), comment, next, lead.Version)),
            default);
    }

    [Test]
    public async Task SyncMatchesByCompanyNameAndMarksGoneWithoutTouchingTheWork()
    {
        var first = await Sync();
        Assert.That((first.Rows, first.Added, first.Updated, first.Gone), Is.EqualTo((3, 3, 0, 0)));
        // «Кто взял в работу» = «Анастасия» took the lead for the manager «Анастасія».
        Assert.That(first.Assigned, Is.EqualTo(1));

        var taken = (await Take(Anastasia, await IdOf(1))).Value;
        // The AI changed the IDs, rewrote the quotes and repeated one ID and one company: rows still match by name.
        _sheet.Rows = [Header, ["5", "2026-08-19", null, "A", "«L&S Distributions»", "Львів", "Склад", "Новий склад"],
            ["5", "2026-08-20", "Олег", "B", "ERC Київ", "Київ", "Склад", "Комплектувальники"],
            ["9", "2026-08-22", null, "B", "erc  київ", "Київ", "Склад", "Повтор"]];
        var second = await Sync();

        await using var db = Context();
        var lead = await db.Leads.SingleAsync(x => x.Id == taken.Id);
        Assert.Multiple(() =>
        {
            Assert.That((second.Rows, second.Added, second.Updated, second.Gone), Is.EqualTo((2, 0, 2, 1)));
            Assert.That(second.Duplicates, Is.EqualTo(1));
            Assert.That(second.Warning, Does.Contain("«ERC Київ» — рядки 3, 4"));
            Assert.That(second.Warning, Does.Contain("«Олег»"));
            Assert.That(second.Assigned, Is.EqualTo(0));
            Assert.That(lead.SourceId, Is.EqualTo(5));
            Assert.That(lead.Company, Is.EqualTo("«L&S Distributions»"));
            Assert.That(lead.OwnerUserId, Is.EqualTo(17));
            Assert.That(lead.SheetNotes, Is.EqualTo("Хто взяв: Анастасия"));
            Assert.That(taken.Stage, Is.EqualTo("InWork"));
            Assert.That(db.Leads.Count(), Is.EqualTo(3));
            Assert.That(db.Leads.Single(x => x.Company == "Suziria Group").InSheet, Is.False);
            Assert.That(db.LeadCategories.Select(x => x.Code).OrderBy(x => x).ToList(), Is.EqualTo(new[] { "A", "B" }));
            Assert.That(db.LeadSyncStates.Single().Rows, Is.EqualTo(2));
            Assert.That(db.LeadSyncStates.Single().Warning, Is.EqualTo(second.Warning));
        });
    }

    [Test]
    public async Task SyncBringsTheSheetCallsOfManagersOnceWithACallTaskForANextContact()
    {
        _sheet.Rows =
        [
            ["ID", "Дата добавления", "Кто взял в работу", "Предприятие", "Результат звонка", "Следующий контакт", "Комментарий"],
            ["1", "2026-08-19", "Вадим", "Альфа", "Вышли на ответственного", "4.11.26", "Просили КП"],
            ["2", "2026-08-19", "Анастасия", "Бета", "Отказ", null, "Є постачальник"],
            ["3", "2026-08-19", "Анастасия", "Гама", "Не звонили", null, null],
            ["4", "2026-08-19", "Анастасия", "Дельта", "Щось нове", null, null],
            ["5", "2026-08-19", null, "Епсилон", "Не дозвонились", null, null],
        ];

        var first = await Sync();
        var second = await Sync();

        await using var db = Context();
        var leads = await db.Leads.Include(x => x.Calls).ToDictionaryAsync(x => x.Company);
        var task = await db.ManagerTasks.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That((first.Assigned, first.Calls), Is.EqualTo((4, 2)));
            Assert.That(first.Warning, Does.Contain("«Щось нове»"));
            Assert.That((second.Assigned, second.Calls), Is.EqualTo((0, 0)), "the same sheet values apply once");

            var alpha = leads["Альфа"];
            Assert.That((alpha.OwnerUserId, alpha.Stage, alpha.CallCount), Is.EqualTo(((int?)18, LeadStage.Talking, 1)));
            Assert.That(alpha.Calls.Single().ResultName, Is.EqualTo("Вийшли на відповідального"));
            Assert.That(alpha.Calls.Single().Comment, Is.EqualTo("У таблиці: «Вышли на ответственного». Просили КП"));
            Assert.That(alpha.Calls.Single().At, Is.EqualTo(new DateTimeOffset(2026, 8, 19, 6, 0, 0, TimeSpan.Zero)));
            Assert.That(alpha.NextContactAt, Is.EqualTo(new DateTimeOffset(2026, 11, 4, 7, 0, 0, TimeSpan.Zero)));
            Assert.That((task.LeadId, task.AssigneeUserId, task.Title), Is.EqualTo(((long?)alpha.Id, 18, "Подзвонити: Альфа")));

            Assert.That((leads["Бета"].Stage, leads["Бета"].NextContactAt), Is.EqualTo((LeadStage.Closed, (DateTimeOffset?)null)));
            Assert.That((leads["Гама"].OwnerUserId, leads["Гама"].CallCount), Is.EqualTo(((int?)17, 0)));
            Assert.That((leads["Дельта"].OwnerUserId, leads["Дельта"].CallCount), Is.EqualTo(((int?)17, 0)));
            Assert.That((leads["Епсилон"].OwnerUserId, leads["Епсилон"].CallCount), Is.EqualTo(((int?)null, 0)));
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
            var byVadym = await new ReleaseLeadHandler(db, _clock)
                .Handle(new ReleaseLeadCommand(Vadym, lead.Id, lead.Version), default);
            Assert.That(byVadym.Status, Is.EqualTo(Ardalis.Result.ResultStatus.Forbidden));
        }

        await using (var db = Context())
        {
            var byDirector = await new ReleaseLeadHandler(db, _clock)
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
        var day = (await new GetMyDayHandler(db, _clock).Handle(new GetMyDayQuery(Anastasia), default)).Value;
        var page = (await new GetLeadsHandler(db, _clock)
            .Handle(new GetLeadsQuery(Anastasia, "київ", "All", null, null, 1, 25), default)).Value;
        var free = (await new GetLeadsHandler(db, _clock)
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
        var stats = (await new GetLeadStatsHandler(db, _clock).Handle(new GetLeadStatsQuery(null, null), default)).Value;

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
