using Crm.Domain.Customers;

namespace Crm.Application.Customers.Models;

public sealed record CustomerDetail(Guid PublicId, string Kind, string DisplayName, string? LegalName, string? TaxIdentity,
    int OwnerId, bool Active, long Revision, DateTimeOffset UpdatedAt, bool CanEdit, bool CanMerge,
    IReadOnlyList<CustomerContactView> Contacts, IReadOnlyList<ExternalIdentityView> ExternalIdentities)
{
    public static CustomerDetail From(Customer customer, CrmActor actor) => new(customer.PublicId, customer.Kind,
        customer.DisplayName, customer.LegalName, customer.TaxIdentity, customer.OwnerId.Value, customer.Active,
        customer.Revision, customer.UpdatedAt, actor.CanAccess(customer) && customer.MergedIntoId is null, actor.IsAdmin,
        customer.Contacts.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Name).Select(x => new CustomerContactView(
            x.PublicId, x.Name, x.Position, x.Active, x.IsPrimary,
            x.Channels.Select(c => new ContactChannelView(c.Kind, c.Value)).ToArray())).ToArray(),
        customer.ExternalIdentities.Select(x => new ExternalIdentityView(x.Source, x.Type, x.ExternalId)).ToArray());
}
