using Crm.Application.Abstractions.Customers;
namespace Crm.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery(CrmActor Actor, string? Search, bool? Active, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<IReadOnlyList<CustomerSummary>>>;
