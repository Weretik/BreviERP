using Ardalis.Result;
using Crm.Application.Features.Customers.Queries.GetCustomers;

namespace Crm.Api.Endpoints.Customers.GetCrmCustomers;

/// <summary>One page of customers: <c>{ value, pagedInfo }</c>, as every paged list.</summary>
internal sealed record CrmCustomerPageResponse(IReadOnlyList<CustomerSummary> Value, PagedInfo PagedInfo);
