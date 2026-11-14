namespace Reminders.Application.Features.Access.Shared;

/// <summary>Signed-in Admin user who acts on reminders; recorded in the journal.</summary>
public sealed record ReminderAdmin(int? UserId, string? Name);
