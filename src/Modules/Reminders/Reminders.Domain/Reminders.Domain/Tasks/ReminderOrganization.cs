namespace Reminders.Domain.Tasks;

/// <summary>Whose obligation the task is. Personal tasks have no organization line in the message.</summary>
public enum ReminderOrganization
{
    None = 0,
    PpBrevi = 1,
    FopSohokon = 2
}
