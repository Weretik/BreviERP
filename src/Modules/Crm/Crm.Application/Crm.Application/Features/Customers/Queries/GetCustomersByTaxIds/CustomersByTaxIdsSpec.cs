using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Queries.GetCustomerLookup;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByTaxIds;

internal sealed class CustomersByTaxIdsSpec : Specification<Customer, CustomerLookup>
{
    public CustomersByTaxIdsSpec(List<string> taxIds)
    {
        Query.AsNoTracking()
            .Where(x => x.MergedIntoId == null && x.Active && x.TaxIdentity != null && taxIds.Contains(x.TaxIdentity.Trim()))
            .OrderBy(x => x.DisplayName);
        Query.Select(x => new CustomerLookup(x.PublicId, x.DisplayName, x.TaxIdentity, x.Active));
    }
}
