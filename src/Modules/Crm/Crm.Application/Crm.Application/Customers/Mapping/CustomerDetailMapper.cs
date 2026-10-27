using Crm.Domain.Customers;
using Crm.Application.Customers.Access;
using Crm.Application.Customers.Contracts.Responses;

namespace Crm.Application.Customers.Mapping;

public static class CustomerDetailMapper
{
    public static CustomerDetail Map(Customer customer, CrmActor actor) => new(customer.PublicId, customer.Kind,
        customer.DisplayName, customer.LegalName, customer.TaxIdentity, customer.OwnerId.Value, customer.Active,
        customer.Revision, customer.UpdatedAt, actor.CanAccess(customer) && customer.MergedIntoId is null, actor.IsAdmin,
        customer.Contacts.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Name).Select(x => new CustomerContactView(
            x.PublicId, x.Name, x.Position, x.Active, x.IsPrimary,
            x.Channels.Select(c => new ContactChannelView(c.Kind, c.Value)).ToArray())).ToArray(),
        customer.ExternalIdentities.Select(x => new ExternalIdentityView(x.Source, x.Type, x.ExternalId)).ToArray());

}
