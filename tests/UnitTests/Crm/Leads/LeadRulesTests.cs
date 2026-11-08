using Crm.Application.Leads.Sheet;
using Crm.Domain.Leads;
using Identity.Domain.Authorization;

namespace UnitTests.Crm.Leads;

/// <summary>Sheet parsing, taking, calls and stages of leads (LEAD-001).</summary>
public sealed class LeadRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 8, 0, 0, TimeSpan.Zero);
    private static readonly LeadPerson Anastasia = new(17, "Анастасія");
    private static readonly LeadPerson Vadym = new(18, "Вадим");

    private static readonly string?[] Header =
    [
        "ID", "Дата добавления", "Кто взял в работу", "Статус", "Категория", "Предприятие", "Регион", "Отрасль",
        "Масштаб / персонал", "Сигнал", "Дата сигнала", "Факт или план", "Ссылка на сигнал", "Почему возможны 50+ единиц",
        "Что предложить", "Официальный сайт", "Открытый контакт", "Кому звонить", "Проверка тендеров", "Повод для звонка",
        "Достоверность", "Результат звонка", "Следующий контакт", "Комментарий", "почта",
    ];

    private static string?[] Row(string id, string company, string? owner = null, string? result = null) =>
    [
        id, "2026-08-19", owner, "Свободно", "a", company, "Київ", "Склад", null, "Новий склад", "14.08.2026",
        "Підтверджений факт", "https://work.ua", "Багато людей", "Жилети", "https://site", "+380 93 405 94 66",
        "Логістика", null, "Уточнити форму", "Висока", result, null, null, "info@site.ua",
    ];

    private static Lead NewLead() =>
        new(LeadSheetParser.Parse([Header, Row("1", "L&S Distributions")]).Rows[0], Now);

    private static LeadCallResult Result(LeadOutcome outcome, int? retryDays = null) =>
        new("Результат", outcome, retryDays, 10);

    [Test]
    public void Parser_finds_columns_by_header_and_keeps_the_manager_columns_as_notes()
    {
        var header = Header.Reverse().ToArray();
        var row = Row("7", " Suziria Group ", "Вадим", "Отправили на почту").Reverse().ToArray();

        var parsed = LeadSheetParser.Parse([header, row]);

        var lead = parsed.Rows.Single();
        Assert.Multiple(() =>
        {
            Assert.That(lead.SourceId, Is.EqualTo(7));
            Assert.That(lead.Company, Is.EqualTo("Suziria Group"));
            Assert.That(lead.Category, Is.EqualTo("A"));
            Assert.That(lead.AddedOn, Is.EqualTo(new DateOnly(2026, 8, 19)));
            Assert.That(lead.SignalDate, Is.EqualTo(new DateOnly(2026, 8, 14)));
            Assert.That(lead.Contacts, Is.EqualTo("+380 93 405 94 66"));
            Assert.That(lead.SheetNotes, Is.EqualTo("Хто взяв: Вадим\nРезультат дзвінка: Отправили на почту"));
        });
    }

    [Test]
    public void Parser_skips_rows_without_an_id_and_stops_on_a_repeated_id()
    {
        var parsed = LeadSheetParser.Parse([Header, Row("", "Без ID"), new string?[25], Row("x", "Текст"), Row("3", "Три")]);
        Assert.That(parsed.Rows.Select(x => x.SourceId), Is.EqualTo(new[] { 3 }));
        Assert.That(parsed.Skipped, Is.EqualTo(2));

        Assert.Throws<LeadSheetFormatException>(() => LeadSheetParser.Parse([Header, Row("3", "А"), Row("3", "Б")]));
        Assert.Throws<LeadSheetFormatException>(() => LeadSheetParser.Parse([["Назва", "Регион"], ["А", "Київ"]]));
    }

    [Test]
    public void Sheet_overwrites_company_fields_but_never_the_calling_work()
    {
        var lead = NewLead();
        lead.Take(Anastasia, Now);
        lead.RecordCall(Anastasia, Result(LeadOutcome.Callback), "Після обіду", Now.AddDays(1), Now);

        var changed = lead.ApplySheet(LeadSheetParser.Parse([Header, Row("1", "L&S Distributions LLC")]).Rows[0], Now);
        var same = lead.ApplySheet(LeadSheetParser.Parse([Header, Row("1", "L&S Distributions LLC")]).Rows[0], Now);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(same, Is.False);
            Assert.That(lead.Company, Is.EqualTo("L&S Distributions LLC"));
            Assert.That(lead.OwnerUserId, Is.EqualTo(Anastasia.UserId));
            Assert.That(lead.Stage, Is.EqualTo(LeadStage.InWork));
            Assert.That(lead.NextContactAt, Is.EqualTo(Now.AddDays(1)));
        });

        lead.MarkGoneFromSheet(Now);
        Assert.That(lead.InSheet, Is.False);
        Assert.That(lead.ApplySheet(LeadSheetParser.Parse([Header, Row("1", "L&S Distributions LLC")]).Rows[0], Now), Is.True);
    }

    [Test]
    public void Only_a_free_lead_can_be_taken_and_release_gives_it_back()
    {
        var lead = NewLead();
        Assert.That(lead.Take(Anastasia, Now), Is.True);
        Assert.That(lead.Take(Anastasia, Now), Is.True);
        Assert.That(lead.Take(Vadym, Now), Is.False);
        Assert.That(lead.Stage, Is.EqualTo(LeadStage.InWork));

        lead.Release(Now);
        Assert.Multiple(() =>
        {
            Assert.That(lead.OwnerUserId, Is.Null);
            Assert.That(lead.Stage, Is.EqualTo(LeadStage.New));
            Assert.That(lead.Take(Vadym, Now), Is.True);
        });
    }

    [TestCase(LeadOutcome.Retry, LeadStage.InWork, false, false)]
    [TestCase(LeadOutcome.Callback, LeadStage.InWork, true, false)]
    [TestCase(LeadOutcome.Talking, LeadStage.Talking, true, false)]
    [TestCase(LeadOutcome.Offer, LeadStage.Offer, true, false)]
    [TestCase(LeadOutcome.Client, LeadStage.Client, false, true)]
    [TestCase(LeadOutcome.Closed, LeadStage.Closed, false, true)]
    public void Outcome_decides_stage_date_and_end(LeadOutcome outcome, LeadStage stage, bool needsDate, bool closes)
    {
        Assert.Multiple(() =>
        {
            Assert.That(outcome.Stage(), Is.EqualTo(stage));
            Assert.That(outcome.NeedsNextContact(), Is.EqualTo(needsDate));
            Assert.That(outcome.Closes(), Is.EqualTo(closes));
        });
    }

    [Test]
    public void A_closing_call_clears_the_next_contact_and_counts_the_call()
    {
        var lead = NewLead();
        lead.Take(Anastasia, Now);
        lead.RecordCall(Anastasia, Result(LeadOutcome.Retry, 2), null, Now.AddDays(2), Now);
        var call = lead.RecordCall(Anastasia, Result(LeadOutcome.Closed), "Свій пошив", Now.AddDays(5), Now);

        Assert.Multiple(() =>
        {
            Assert.That(call.NextContactAt, Is.Null);
            Assert.That(lead.NextContactAt, Is.Null);
            Assert.That(lead.CallCount, Is.EqualTo(2));
            Assert.That(lead.IsOpen, Is.False);
            Assert.That(lead.LastResultName, Is.EqualTo("Результат"));
        });
    }

    [Test]
    public void Retry_days_belong_only_to_retry_results()
    {
        Assert.That(new LeadCallResult("Не дозвонились", LeadOutcome.Retry, null, 1).RetryDays, Is.EqualTo(1));
        Assert.That(new LeadCallResult("Передзвонити", LeadOutcome.Callback, 3, 1).RetryDays, Is.Null);
    }

    [Test]
    public void Leads_pages_are_in_the_access_catalogue_with_their_defaults()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.Leads],
                Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.LeadStats],
                Is.EqualTo(AccessAction.View | AccessAction.Create | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager), Does.Not.ContainKey(AccessCatalog.LeadStats));
        });
    }
}
