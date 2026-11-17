using ActivityLog.Contracts.Activity;
using Production.Domain.Settings;
using Production.Domain.Staff;

namespace Production.Application.Features.Activity.EventHandlers;

/// <summary>How workshop staff and the workshop Telegram settings read in «Журнал дій» (ACT-001).</summary>
public sealed class WorkshopActivity : IActivityDescriber
{
    public const string EmployeeSubject = "workshop-employee";
    public const string SettingsSubject = "workshop-settings";

    public ActivityDescription? Describe(object domainEvent) => domainEvent switch
    {
        WorkshopEmployeeSaved { Before: null } e => Employee(e.Employee, ActivityActions.Created, e.Employee.Position, []),
        WorkshopEmployeeSaved { Before: { } before } e => new ActivityChanges()
                .Add("ПІБ", before.FullName, e.Employee.FullName)
                .Add("Посада", before.Position, e.Employee.Position) is { Any: true } changes
            ? Employee(e.Employee, ActivityActions.Changed, null, changes.Items)
            : null,
        WorkshopEmployeeActivated e => Employee(e.Employee, ActivityActions.Changed,
            e.Employee.IsActive ? "Повернуто до роботи" : "Більше не працює", []),
        WorkshopSettingsChanged e => new ActivityDescription(ActivityActions.Changed, SettingsSubject,
            "Налаштування цеху", "settings", "Telegram цеху",
            e.TelegramEnabled ? "Надсилання в Telegram увімкнено" : "Надсилання в Telegram вимкнено", [], "Production"),
        _ => null,
    };

    private static ActivityDescription Employee(WorkshopEmployee employee, string action, string? summary,
        IReadOnlyList<ActivityChangeItem> changes) =>
        new(action, EmployeeSubject, "Працівник цеху", employee.Id.Value.ToString(CultureInfo.InvariantCulture),
            employee.FullName, summary, changes, "Production");
}
