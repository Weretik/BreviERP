using Crm.Application.Customers.GetCustomers.DTOs;
using Crm.Application.Customers.Persistence;
using Crm.Application.Customers.GetCustomers.Specifications;
namespace Crm.Application.Customers.GetCustomers;

public sealed class GetCustomersQueryHandler(ICustomerReadRepository repository)
    : IQueryHandler<GetCustomersQuery, PagedResult<IReadOnlyList<CustomerSummary>>>
{
    public async ValueTask<PagedResult<IReadOnlyList<CustomerSummary>>> Handle(GetCustomersQuery query, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(query.Actor.UserId);
        var count = await repository.CountAsync(new CustomerCountSpec(query.Actor, query.Search, query.Active, query.Page, query.PageSize), cancellationToken);
        var rows = await repository.ListAsync(new CustomerPageSpec(query.Actor, query.Search, query.Active, query.Page, query.PageSize), cancellationToken);
        return new PagedResult<IReadOnlyList<CustomerSummary>>(new PagedInfo(query.Page, query.PageSize,
            (long)Math.Ceiling(count / (double)query.PageSize), count), rows);
    }
}
