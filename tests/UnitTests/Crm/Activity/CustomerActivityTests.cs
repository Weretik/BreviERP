using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Crm.Application.Features.Activity.EventHandlers;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;

namespace UnitTests.Crm.Activity;

/// <summary>Client actions read as «Журнал дій» entries (ACT-001).</summary>
public sealed class CustomerActivityTests
{
    private readonly CustomerActivity _describer = new();

    private static Customer NewCustomer() =>
        Customer.Create("organization", "ТОВ «Агро»", null, null, new CrmUserId(1)).Value;

    private ActivityDescription? Last(IHasDomainEvents source) => _describer.Describe(source.DomainEvents.Last());

    [Test]
    public void Creating_a_client_is_listed_with_its_name()
    {
        var customer = NewCustomer();

        var entry = Last(customer);
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Created));
        Assert.That(entry.SubjectTitle, Is.EqualTo("ТОВ «Агро»"));
        Assert.That(entry.SubjectId, Is.EqualTo(customer.PublicId.ToString()));
    }

    [Test]
    public void Revising_lists_the_changed_details()
    {
        var customer = NewCustomer();

        customer.Revise(customer.Revision, "organization", "ТОВ «Агро-Плюс»", null, "12345678", true);

        Assert.That(Last(customer)!.Changes, Is.EqualTo(new[]
        {
            new ActivityChangeItem("Назва", "ТОВ «Агро»", "ТОВ «Агро-Плюс»"),
            new ActivityChangeItem("ЄДРПОУ / ІПН", null, "12345678"),
        }));
    }
}
