using Ardalis.Specification;
using Crm.Application.Customers.Models;
using Crm.Domain.Customers;
namespace Crm.Application.Customers.GetCustomer.Specifications;

public sealed class CustomerDetailSpec : Specification<Customer, CustomerDetail>
{
    public CustomerDetailSpec(Guid publicId, CrmActor actor)
    {
        Query.AsNoTracking().Where(x => x.PublicId == publicId && x.MergedIntoId == null && (actor.IsAdmin || x.OwnerId == new Crm.Domain.Customers.Identifiers.CrmUserId(actor.UserId)));
        Query.Select(x => new CustomerDetail(x.PublicId, x.Kind, x.DisplayName, x.LegalName, x.TaxIdentity,
            x.OwnerId.Value, x.Active, x.Revision, x.UpdatedAt, true, actor.IsAdmin,
            x.Contacts.OrderByDescending(c => c.IsPrimary).ThenBy(c => c.Name).Select(c => new CustomerContactView(c.PublicId,
                c.Name, c.Position, c.Active, c.IsPrimary, c.Channels.Select(v => new ContactChannelView(v.Kind, v.Value)).ToList())).ToList(),
            x.ExternalIdentities.Select(i => new ExternalIdentityView(i.Source, i.Type, i.ExternalId)).ToList()));
    }
}
