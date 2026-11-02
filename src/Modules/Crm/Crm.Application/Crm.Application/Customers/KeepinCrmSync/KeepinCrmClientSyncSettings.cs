using Crm.Domain.Customers.Identifiers;

namespace Crm.Application.Customers.KeepinCrmSync;

/// <summary>Owner of clients created from KeepinCRM (config KeepinCrm:ClientOwnerUserId, default 1).</summary>
public sealed record KeepinCrmClientSyncSettings(CrmUserId Owner);
