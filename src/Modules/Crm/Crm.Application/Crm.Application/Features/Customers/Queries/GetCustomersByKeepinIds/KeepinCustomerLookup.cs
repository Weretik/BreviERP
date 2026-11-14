using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByKeepinIds;

public sealed record KeepinCustomerLookup(string KeepinId, Guid PublicId, string DisplayName, string? TaxIdentity,
    bool Active);
