using Microsoft.EntityFrameworkCore;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Schedules;

namespace UnitTests.Reminders;

public sealed class ReminderDispatcherTests
{
    // Monday 2026-11-02 09:00 in Kyiv (UTC+2 after the October clock change).
    private static readonly DateTimeOffset Monday = new(2026, 11, 2, 7, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task Sends_one_card_per_open_period_and_a_rerun_sends_nothing()
    {
        var h = new ReminderTestHarness(Monday);
        var chat = await h.ApprovedGroupAsync();
        await h.TaskAsync(chat, DueRule.DayOfMonth(5, 1), new DateOnly(2026, 10, 25));
        await h.TaskAsync(chat, DueRule.DayOfMonth(1, 1), new DateOnly(2026, 10, 25), "Порахувати зарплату цеху");
        await h.TaskAsync(chat, DueRule.DayOfMonth(20, 1), new DateOnly(2026, 10, 25), "Декларація з ПДВ"); // window from 15.11

        var first = await h.Dispatcher().RunAsync(CancellationToken.None);
        var second = await h.Dispatcher().RunAsync(CancellationToken.None);

        Assert.That(first.Sent, Is.EqualTo(2));
        Assert.That(first.ActiveTasks, Is.EqualTo(3));
        Assert.That(first.NotDueYet, Is.EqualTo(1), "ПДВ window opens on 15.11");
        Assert.That(second.Sent, Is.Zero);
        Assert.That(second.AlreadySent, Is.EqualTo(2));
        Assert.That(h.Telegram.Sent, Has.Count.EqualTo(2));
        Assert.That(h.Telegram.Sent.All(x => x.ChatId == ReminderTestHarness.GroupTelegramId), Is.True);
        Assert.That(h.Telegram.Sent[0].Text, Does.Contain("Прострочено на 1 дн.")); // due Sunday 01.11
        Assert.That(h.Telegram.Sent[1].Text, Does.Contain("📅 Термін до: 05.11.2026").And.Contain("🏢 ПП «Бреві»").And.Contain("жовтень 2026"));
        Assert.That(h.Telegram.Sent[1].Keyboard!.Rows[0].Select(x => x.Text), Is.EqualTo(new[] { "✅ Виконано", "⏰ Нагадати пізніше" }));
    }

    [Test]
    public async Task Weekend_run_sends_nothing()
    {
        var h = new ReminderTestHarness(Monday.AddDays(-1));
        var chat = await h.ApprovedGroupAsync();
        await h.TaskAsync(chat, DueRule.DayOfMonth(5, 1), new DateOnly(2026, 10, 25));
        var summary = await h.Dispatcher().RunAsync(CancellationToken.None);
        Assert.That(summary.WorkingDay, Is.False);
        Assert.That(h.Telegram.Sent, Is.Empty);
    }

    [Test]
    public async Task Next_day_removes_yesterday_buttons_before_the_new_card()
    {
        var h = new ReminderTestHarness(Monday);
        var chat = await h.ApprovedGroupAsync();
        await h.TaskAsync(chat, DueRule.DayOfMonth(5, 1), new DateOnly(2026, 10, 25));
        await h.Dispatcher().RunAsync(CancellationToken.None);
        h.Time.Advance(TimeSpan.FromDays(1));
        await h.Dispatcher().RunAsync(CancellationToken.None);

        Assert.That(h.Telegram.Sent, Has.Count.EqualTo(2));
        var removed = h.Telegram.KeyboardEdits.Single();
        Assert.That(removed.MessageId, Is.EqualTo(h.Telegram.Sent[0].MessageId));
        Assert.That(removed.Keyboard, Is.Null);
        Assert.That(await h.Db.SentMessages.CountAsync(x => x.HasButtons), Is.EqualTo(1));
    }

    [Test]
    public async Task Unapproved_chat_and_disabled_task_receive_nothing()
    {
        var h = new ReminderTestHarness(Monday);
        var chat = await h.ApprovedGroupAsync();
        chat.Configure(null, false, h.Clock.Now);
        await h.Db.SaveChangesAsync();
        await h.TaskAsync(chat, DueRule.DayOfMonth(5, 1), new DateOnly(2026, 10, 25));
        var summary = await h.Dispatcher().RunAsync(CancellationToken.None);
        Assert.That(h.Telegram.Sent, Is.Empty);
        Assert.That(summary.ChatUnavailable, Is.EqualTo(1));
        Assert.That(await h.Db.Occurrences.CountAsync(x => x.Status == ReminderOccurrenceStatus.Open), Is.EqualTo(1));
    }
}
