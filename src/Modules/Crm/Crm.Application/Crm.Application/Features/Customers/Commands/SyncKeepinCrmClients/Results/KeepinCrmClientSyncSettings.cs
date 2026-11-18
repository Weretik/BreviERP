using Crm.Domain.Customers.Identifiers;
using BuildingBlocks.Domain.Identifiers.People;

namespace Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients.Results;

/// <summary>Owner of clients created from KeepinCRM (config KeepinCrm:ClientOwnerUserId, default 1).</summary>
public sealed record KeepinCrmClientSyncSettings(UserId Owner);
