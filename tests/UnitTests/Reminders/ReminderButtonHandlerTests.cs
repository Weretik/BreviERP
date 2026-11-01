using Microsoft.EntityFrameworkCore;
using Reminders.Application.Messages;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Schedules;

namespace UnitTests.Reminders;

public sealed class ReminderButtonHandlerTests
{
    private static readonly DateTimeOffset Monday = new(2026, 11, 2, 7, 0, 0, TimeSpan.Zero);

    private static async Task<(ReminderTestHarness H, long MessageId, long OccurrenceId)> SentCardAsync()
    {
        var h = new ReminderTestHarness(Monday);
        var chat = await h.ApprovedGroupAsync();
        await h.TaskAsync(chat, DueRule.DayOfMonth(20, 1), new DateOnly(2026, 10, 25));
        h.Time.Advance(TimeSpan.FromDays(14)); // Monday 16.11, window started 15.11
        await h.Dispatcher().RunAsync(CancellationToken.None);
        var card = h.Telegram.Sent.Single();
        var occurrence = await h.Db.Occurrences.SingleAsync();
        return (h, card.MessageId, occurrence.Id.Value);
    }

    [Test]
    public async Task Done_records_who_and_when_and_the_second_press_is_told_it_is_closed()
    {
        var (h, messageId, id) = await SentCardAsync();
        await h.Buttons().HandleAsync(ReminderTestHarness.Press(messageId, $"rm:{id}:done"), CancellationToken.None);
        await h.Buttons().HandleAsync(ReminderTestHarness.Press(messageId, $"rm:{id}:done", 43, "Марія"), CancellationToken.None);

        var occurrence = await h.Db.Occurrences.Include(x => x.Actions).SingleAsync();
        Assert.That(occurrence.Status, Is.EqualTo(ReminderOccurrenceStatus.Done));
        Assert.That(occurrence.ClosedBy, Is.EqualTo(new ReminderActor("Олена", 42, null)));
        Assert.That(occurrence.Actions, Has.Count.EqualTo(1));
        Assert.That(h.Telegram.Edited[0].Text, Does.Contain("Закрито ✅").And.Contain("👤 Олена, 16.11.2026 09:00"));
        Assert.That(h.Telegram.Edited[0].Keyboard, Is.Null);
        Assert.That(h.Telegram.Answers[1].Text, Does.StartWith("Вже закрито: Олена"));
    }

    [Test]
    public async Task Later_offers_three_dates_and_monday_postpones_without_moving_the_deadline()
    {
        var (h, messageId, id) = await SentCardAsync();
        await h.Buttons().HandleAsync(ReminderTestHarness.Press(messageId, $"rm:{id}:later"), CancellationToken.None);
        Assert.That(h.Telegram.KeyboardEdits.Last().Keyboard!.Rows[0].Select(x => x.Text),
            Is.EqualTo(new[] { "Завтра", "Через 2 дні", "У понеділок" }));

        await h.Buttons().HandleAsync(ReminderTestHarness.Press(messageId, $"rm:{id}:s3"), CancellationToken.None);
        var occurrence = await h.Db.Occurrences.SingleAsync();
        Assert.That(occurrence.SnoozedUntil, Is.EqualTo(new DateOnly(2026, 11, 20))); // capped at the deadline (Friday)
        Assert.That(occurrence.DueDate, Is.EqualTo(new DateOnly(2026, 11, 20)));
        Assert.That(h.Telegram.Edited.Last().Text, Does.Contain("Перенесено ⏰").And.Contain("Наступне нагадування: 20.11.2026"));

        h.Time.Advance(TimeSpan.FromDays(1));
        await h.Dispatcher().RunAsync(CancellationToken.None);
        Assert.That(h.Telegram.Sent, Has.Count.EqualTo(1), "postponed period is quiet until 20.11");
        h.Time.Advance(TimeSpan.FromDays(3));
        await h.Dispatcher().RunAsync(CancellationToken.None);
        Assert.That(h.Telegram.Sent.Last().Text, Does.Contain("Переносили: Олена").And.Contain("Термін сьогодні"));
    }

    [Test]
    public async Task Unknown_payload_is_answered_without_changes()
    {
        var (h, messageId, _) = await SentCardAsync();
        await h.Buttons().HandleAsync(ReminderTestHarness.Press(messageId, "rm:999:done"), CancellationToken.None);
        await h.Buttons().HandleAsync(ReminderTestHarness.Press(messageId, "garbage"), CancellationToken.None);
        Assert.That(h.Telegram.Answers.Select(x => x.Text), Is.EqualTo(new[] { "Нагадування не знайдено", null }));
        Assert.That(await h.Db.Occurrences.SingleAsync(), Has.Property(nameof(ReminderOccurrence.Status)).EqualTo(ReminderOccurrenceStatus.Open));
    }

    [TestCase("rm:12:s2")]
    [TestCase("rm:12:back")]
    public void Callback_data_round_trips(string value)
    {
        Assert.That(ReminderCallbackData.TryParse(value, out var data), Is.True);
        Assert.That(data.ToString(), Is.EqualTo(value));
    }
}
