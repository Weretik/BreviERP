using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomerLookup;

/// <summary>
/// Client names for another module that has its own access rule (for example «Бухгалтерія», open to the director only):
/// every client regardless of its manager, merged ones never. <c>Ids</c> picks exact clients (archived too);
/// otherwise <c>Search</c> looks in the name and ЄДРПОУ/ІПН, and <c>ActiveOnly</c> leaves archived ones out.
/// </summary>
public sealed record GetCustomerLookupQuery(string? Search, IReadOnlyCollection<Guid>? Ids, bool ActiveOnly, int Page,
    int PageSize) : IQuery<CustomerLookupPage>;
