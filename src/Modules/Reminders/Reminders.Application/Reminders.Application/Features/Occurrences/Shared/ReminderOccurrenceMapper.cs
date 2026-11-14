using Reminders.Domain.Occurrences;
using Reminders.Domain.Tasks;
using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Occurrences.Shared;

public static class ReminderOccurrenceMapper
{
    public static ReminderOccurrenceResponse ToResponse(ReminderOccurrence occurrence, ReminderTask task) => new(
        occurrence.Id.Value,
        task.Id.Value,
        task.Title,
        ReminderCodes.Code(ReminderCodes.Organizations, task.Organization),
        occurrence.Period.Label,
        occurrence.DueDate,
        task.Schedule.WindowStart(occurrence.DueDate),
        ReminderCodes.Status(occurrence.Status),
        occurrence.SnoozedUntil,
        occurrence.ClosedBy is null ? null : Actor(occurrence.ClosedBy),
        occurrence.ClosedAt,
        occurrence.Actions.OrderByDescending(x => x.At)
            .Select(x => new ReminderActionResponse(ReminderCodes.Action(x.Kind), Actor(x.Actor), x.At, x.SnoozedUntil)).ToArray());

    private static ReminderActorResponse Actor(ReminderActor actor) => new(actor.Name, actor.TelegramUserId, actor.AdminUserId);
}
