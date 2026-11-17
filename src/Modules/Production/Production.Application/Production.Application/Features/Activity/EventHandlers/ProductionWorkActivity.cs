using ActivityLog.Contracts.Activity;
using Production.Domain.Work;

namespace Production.Application.Features.Activity.EventHandlers;

/// <summary>How recorded and voided workshop work reads in «Журнал дій» (ACT-001).</summary>
public sealed class ProductionWorkActivity : IActivityDescriber
{
    public const string Subject = "production-work";

    public ActivityDescription? Describe(object domainEvent) => domainEvent is ProductionWorkRecorded e
        ? new ActivityDescription(e.Voided ? ActivityActions.Voided : ActivityActions.Created, Subject, "Робота цеху",
            e.Work.Id.Value.ToString(CultureInfo.InvariantCulture),
            $"{e.Work.EmployeeName} · {e.Work.WorkDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)}",
            e.Voided ? "Причина: " + e.Work.VoidReason : Summary(e.Work), [], "Production")
        : null;

    private static string Summary(ProductionWork work) =>
        (work.Kind == ProductionWorkKind.Cutting ? "Крій" : "Пошиття") + ": "
        + work.Quantity.ToString(CultureInfo.InvariantCulture) + " шт."
        + (work.OverPlan ? " понад план (" + work.Reason + ")" : "");
}
