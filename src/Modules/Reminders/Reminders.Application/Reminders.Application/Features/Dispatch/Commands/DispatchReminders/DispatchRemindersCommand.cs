using Reminders.Application.Features.Dispatch.Shared;

namespace Reminders.Application.Features.Dispatch.Commands.DispatchReminders;

/// <summary>The daily Telegram reminder run (brevierp-reminders); safe to repeat the same day.</summary>
public sealed record DispatchRemindersCommand : ICommand<Result<ReminderDispatchSummary>>;
