using Reference.Domain.Suppliers;

namespace Reference.Application.Contracts.KeepinCrm;

/// <summary>Reads every supplier from KeepinCRM («Постачальники» in KeepinCRM).</summary>
public interface IKeepinCrmSupplierSource
{
    /// <exception cref="KeepinCrmUnavailableException">KeepinCRM is not configured or did not answer correctly.</exception>
    Task<IReadOnlyList<KeepinCrmSupplierSnapshot>> GetAllAsync(CancellationToken cancellationToken);
}

/// <summary>KeepinCRM is not configured or its answer cannot be used; nothing was changed.</summary>
public sealed class KeepinCrmUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
