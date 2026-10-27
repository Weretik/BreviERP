using Crm.Application.Customers.GetCustomers.DTOs;
using Crm.Application.Customers.Access;
namespace Crm.Application.Customers.GetCustomers;

public sealed record GetCustomersQuery(CrmActor Actor, string? Search, bool? Active, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<IReadOnlyList<CustomerSummary>>>;
