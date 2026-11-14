using Reference.Domain.Suppliers;

namespace Reference.Application.Abstractions.KeepinCrm;

/// <summary>Reads every supplier from KeepinCRM («Постачальники» in KeepinCRM).</summary>
public interface IKeepinCrmSupplierSource
{
    /// <exception cref="KeepinCrmUnavailableException">KeepinCRM is not configured or did not answer correctly.</exception>
    Task<IReadOnlyList<KeepinCrmSupplierSnapshot>> GetAllAsync(CancellationToken cancellationToken);
}
