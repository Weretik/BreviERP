using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomerLookup;

public sealed record CustomerLookup(Guid PublicId, string DisplayName, string? TaxIdentity, bool Active);
