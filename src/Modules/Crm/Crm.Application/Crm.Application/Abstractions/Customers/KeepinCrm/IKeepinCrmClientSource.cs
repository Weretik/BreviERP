using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients.Results;
namespace Crm.Application.Abstractions.Customers.KeepinCrm;

/// <summary>Reads every KeepinCRM client, page by page. Throws <see cref="KeepinCrmClientsUnavailableException"/>.</summary>
public interface IKeepinCrmClientSource
{
    Task<IReadOnlyList<KeepinCrmClientSnapshot>> GetAllAsync(CancellationToken cancellationToken);
}

/// <summary>KeepinCRM could not be read; Brevi data stays unchanged.</summary>
public sealed class KeepinCrmClientsUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
