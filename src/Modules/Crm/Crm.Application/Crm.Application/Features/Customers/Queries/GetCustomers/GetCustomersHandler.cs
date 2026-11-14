using Crm.Application.Abstractions.Customers;
namespace Crm.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersHandler(ICustomerReadRepository repository)
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
