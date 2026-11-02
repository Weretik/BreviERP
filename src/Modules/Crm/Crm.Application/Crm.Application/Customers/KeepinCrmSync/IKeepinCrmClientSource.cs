namespace Crm.Application.Customers.KeepinCrmSync;

/// <summary>Reads every KeepinCRM client, page by page. Throws <see cref="KeepinCrmClientsUnavailableException"/>.</summary>
public interface IKeepinCrmClientSource
{
    Task<IReadOnlyList<KeepinCrmClientSnapshot>> GetAllAsync(CancellationToken cancellationToken);
}

/// <summary>KeepinCRM could not be read; Brevi data stays unchanged.</summary>
public sealed class KeepinCrmClientsUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
