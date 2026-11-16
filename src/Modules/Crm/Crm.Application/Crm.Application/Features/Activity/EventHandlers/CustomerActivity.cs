using ActivityLog.Contracts.Activity;
using Crm.Domain.Customers;

namespace Crm.Application.Features.Activity.EventHandlers;

/// <summary>How client actions read in «Журнал дій» (ACT-001); the KeepinCRM sync is not listed.</summary>
public sealed class CustomerActivity : IActivityDescriber
{
    public const string Subject = "customer";

    public ActivityDescription? Describe(object domainEvent) => domainEvent switch
    {
        CustomerSaved { Before: null } e => Of(e.Customer, ActivityActions.Created, null, []),
        CustomerSaved e => Changes(e.Before!, e.Customer.Snapshot()) is { Any: true } changes
            ? Of(e.Customer, ActivityActions.Changed, null, changes.Items)
            : null,
        CustomerContactSaved e => Of(e.Customer, ActivityActions.Changed,
            (e.IsNew ? "Додано контакт: " : "Змінено контакт: ") + e.ContactName, []),
        CustomersMerged e => Of(e.Survivor, ActivityActions.Changed, "Об'єднано з клієнтом «" + e.MergedName + "»", []),
        _ => null,
    };

    private static ActivityChanges Changes(CustomerSnapshot before, CustomerSnapshot after) => new ActivityChanges()
        .Add("Тип", Kind(before.Kind), Kind(after.Kind))
        .Add("Назва", before.DisplayName, after.DisplayName)
        .Add("Юридична назва", before.LegalName, after.LegalName)
        .Add("ЄДРПОУ / ІПН", before.TaxIdentity, after.TaxIdentity)
        .Add("Активний", before.Active, after.Active);

    private static string Kind(string kind) => kind == "person" ? "Фізична особа" : "Організація";

    private static ActivityDescription Of(Customer customer, string action, string? summary,
        IReadOnlyList<ActivityChangeItem> changes) =>
        new(action, Subject, "Клієнт", customer.PublicId.ToString(), customer.DisplayName, summary, changes, "Crm");
}
