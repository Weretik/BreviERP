using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Queries.GetCustomerLookup;
using Crm.Application.Abstractions.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByTaxIds;

public sealed class GetCustomersByTaxIdsHandler(ICustomerReadRepository repository)
    : IQueryHandler<GetCustomersByTaxIdsQuery, IReadOnlyList<CustomerLookup>>
{
    public async ValueTask<IReadOnlyList<CustomerLookup>> Handle(GetCustomersByTaxIdsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.TaxIds.Count == 0) return [];
        var wanted = query.TaxIds.Select(x => x.Trim()).Distinct().ToList();
        return await repository.ListAsync(new CustomersByTaxIdsSpec(wanted), cancellationToken);
    }
}
