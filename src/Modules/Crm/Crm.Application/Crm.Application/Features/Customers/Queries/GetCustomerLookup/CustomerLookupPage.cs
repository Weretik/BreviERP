using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomerLookup;

public sealed record CustomerLookupPage(IReadOnlyList<CustomerLookup> Items, int Total);
