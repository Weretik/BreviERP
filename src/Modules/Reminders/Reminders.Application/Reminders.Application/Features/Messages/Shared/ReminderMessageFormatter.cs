using System.Text;
using Reminders.Application.Abstractions;
using Reminders.Domain;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Tasks;
using Reminders.Application.Abstractions.Telegram;

namespace Reminders.Application.Features.Messages.Shared;

/// <summary>Builds the Ukrainian reminder card and its buttons (Telegram HTML).</summary>
public static class ReminderMessageFormatter
{
    private static readonly string[] Weekdays =
        ["неділя", "понеділок", "вівторок", "середа", "четвер", "п'ятниця", "субота"];

    public static string OrganizationLabel(ReminderOrganization organization) => organization switch
    {
        ReminderOrganization.PpBrevi => "ПП «Бреві»",
        ReminderOrganization.FopSohokon => "ФОП Согоконь",
        _ => string.Empty
    };

    public static string Text(ReminderTask task, ReminderOccurrence occurrence, ReminderNotice notice, DateOnly today)
    {
        var text = new StringBuilder("🔔 <b>Нагадування</b>\n");
        var organization = OrganizationLabel(task.Organization);
        if (organization.Length > 0) text.Append("🏢 ").Append(Html(organization)).Append('\n');
        text.Append("📝 Задача: <b>").Append(Html(task.Title)).Append("</b>\n");
        text.Append("🗓 Період: ").Append(occurrence.Period.Label).Append('\n');
        text.Append("📅 Термін до: ").Append(Date(occurrence.DueDate)).Append('\n');

        if (occurrence.Status == ReminderOccurrenceStatus.Done)
        {
            text.Append("📍 Статус: Закрито ✅\n");
            text.Append("👤 ").Append(Html(occurrence.ClosedBy!.Name)).Append(", ").Append(DateTime(occurrence.ClosedAt!.Value));
            return text.ToString();
        }

        if (occurrence.IsPostponedOn(today))
        {
            text.Append("📍 Статус: Перенесено ⏰\n");
            text.Append("👤 ").Append(Html(occurrence.PostponedBy!.Name)).Append(", ").Append(DateTime(occurrence.PostponedAt!.Value)).Append('\n');
            text.Append("🔁 Наступне нагадування: ").Append(Date(occurrence.SnoozedUntil!.Value));
            return text.ToString();
        }

        text.Append("📍 Статус: Активно");
        if (occurrence.PostponedBy is not null)
            text.Append("\n⏰ Переносили: ").Append(Html(occurrence.PostponedBy.Name)).Append(", ").Append(DateTime(occurrence.PostponedAt!.Value));
        var line = NoticeLine(notice, occurrence.DueDate);
        if (line is not null) text.Append('\n').Append(line);
        return text.ToString();
    }

    public static TelegramKeyboard MainKeyboard(ReminderOccurrence occurrence) => new([
    [
        new TelegramButton("✅ Виконано", new ReminderCallbackData(occurrence.Id.Value, ReminderCallbackAction.Done).ToString()),
        new TelegramButton("⏰ Нагадати пізніше", new ReminderCallbackData(occurrence.Id.Value, ReminderCallbackAction.Later).ToString())
    ]]);

    public static TelegramKeyboard SnoozeKeyboard(ReminderOccurrence occurrence)
    {
        var id = occurrence.Id.Value;
        return new TelegramKeyboard([
        [
            new TelegramButton("Завтра", new ReminderCallbackData(id, ReminderCallbackAction.Snooze, SnoozeOption.Tomorrow).ToString()),
            new TelegramButton("Через 2 дні", new ReminderCallbackData(id, ReminderCallbackAction.Snooze, SnoozeOption.TwoDays).ToString()),
            new TelegramButton("У понеділок", new ReminderCallbackData(id, ReminderCallbackAction.Snooze, SnoozeOption.Monday).ToString())
        ],
        [new TelegramButton("↩ Назад", new ReminderCallbackData(id, ReminderCallbackAction.Back).ToString())]]);
    }

    public static string Date(DateOnly date) => date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    public static string DateTime(DateTimeOffset instant) =>
        ReminderTime.ToKyiv(instant).ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

    private static string? NoticeLine(ReminderNotice notice, DateOnly dueDate) => notice.Kind switch
    {
        ReminderNoticeKind.DueToday => "⚠️ Термін сьогодні",
        ReminderNoticeKind.Overdue => $"⚠️ Прострочено на {notice.OverdueDays} дн.",
        ReminderNoticeKind.DueOnNonWorkingDay =>
            $"⚠️ Термін припадає на вихідний ({Weekdays[(int)dueDate.DayOfWeek]}, {Date(dueDate)}), сьогодні останній робочий день до нього",
        _ => null
    };

    private static string Html(string value) =>
        value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
