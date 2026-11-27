using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomerInternalId;

internal sealed class CustomerInternalIdSpec : SingleResultSpecification<Customer, long?>
{
    public CustomerInternalIdSpec(Guid publicId)
    {
        Query.AsNoTracking().Where(x => x.PublicId == publicId);
        Query.Select(x => (long?)x.Id.Value);
    }
}
