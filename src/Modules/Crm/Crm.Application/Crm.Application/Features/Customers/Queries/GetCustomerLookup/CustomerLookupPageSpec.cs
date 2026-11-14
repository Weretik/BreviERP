using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomerLookup;

internal sealed class CustomerLookupPageSpec : Specification<Customer, CustomerLookup>
{
    public CustomerLookupPageSpec(GetCustomerLookupQuery query)
    {
        CustomerLookupSpec.Filter(Query, query, paged: true);
        Query.Select(x => new CustomerLookup(x.PublicId, x.DisplayName, x.TaxIdentity, x.Active));
    }
}
