using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByKeepinIds;

internal sealed record KeepinCustomerRow(Guid PublicId, string DisplayName, string? TaxIdentity, bool Active,
    List<string> KeepinIds);
