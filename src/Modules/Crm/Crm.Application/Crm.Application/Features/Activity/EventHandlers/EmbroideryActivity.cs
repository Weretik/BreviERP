using ActivityLog.Contracts.Activity;
using Crm.Domain.Embroidery;
using Crm.Domain.Embroidery.Saved;

namespace Crm.Application.Features.Activity.EventHandlers;

/// <summary>How «Ціна вишивки» actions read in «Журнал дій» (ACT-001): saved calculations and the rates.</summary>
public sealed class EmbroideryActivity : IActivityDescriber
{
    public const string Subject = "embroidery-calculation";
    public const string SettingsSubject = "embroidery-settings";

    public ActivityDescription? Describe(object domainEvent) => domainEvent switch
    {
        EmbroideryCalculationSaved e => Of(ActivityActions.Created, e.Calculation.Id, e.Calculation.Name,
            e.Calculation.CustomerName is { } customer ? "Клієнт: " + customer : null),
        EmbroideryMockupSet e => Of(ActivityActions.Changed, e.Calculation.Id, e.Calculation.Name,
            e.Calculation.MockupMediaFileId is null ? "Макет прибрано" : "Додано макет"),
        EmbroideryCalculationDeleted e => Of(ActivityActions.Deleted, e.CalculationId, e.Name, null),
        EmbroiderySettingsChanged e => new ActivityDescription(ActivityActions.Changed, SettingsSubject,
            "Налаштування вишивки", "settings", "Тарифи вишивки",
            "Збережено версію " + e.Version.ToString(CultureInfo.InvariantCulture), [], "Crm"),
        _ => null,
    };

    private static ActivityDescription Of(string action, long id, string name, string? summary) =>
        new(action, Subject, "Розрахунок вишивки", id.ToString(CultureInfo.InvariantCulture),
            EmbroiderySavedCalculation.NumberOf(id) + " · " + (string.IsNullOrWhiteSpace(name) ? "Без назви" : name),
            summary, [], "Crm");
}
