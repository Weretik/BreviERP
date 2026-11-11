namespace Reminders.Application.Contracts;

/// <param name="ReceivesSiteRequests">«Заявки з сайту» (LEAD-002): null keeps the current choice.</param>
public sealed record ReminderChatInput(string? DisplayName, bool IsApproved, bool? ReceivesSiteRequests = null);
