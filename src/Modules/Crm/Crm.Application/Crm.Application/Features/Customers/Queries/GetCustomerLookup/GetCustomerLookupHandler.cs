using Crm.Domain.Customers;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Features.Customers.Queries.GetCustomerLookup.Specs;

namespace Crm.Application.Features.Customers.Queries.GetCustomerLookup;

public sealed class GetCustomerLookupHandler(ICustomerReadRepository repository)
    : IQueryHandler<GetCustomerLookupQuery, CustomerLookupPage>
{
    public async ValueTask<CustomerLookupPage> Handle(GetCustomerLookupQuery query, CancellationToken cancellationToken)
    {
        var total = await repository.CountAsync(new CustomerLookupSpec(query, paged: false), cancellationToken);
        var rows = await repository.ListAsync(new CustomerLookupPageSpec(query), cancellationToken);
        return new CustomerLookupPage(rows, total);
    }
}
