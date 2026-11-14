using Production.Application.Abstractions;

using Reference.Contracts.FabricColors;
using Reference.Contracts.Fabrics;
using Reference.Contracts.PricingParameters;

namespace Production.Infrastructure.Adapters.Reference;

/// <summary>Fabrics of «Тканини» (Reference) for the codes and names an order item keeps.</summary>
internal sealed class ProductionFabricReader(IFabricDirectory fabrics) : IProductionFabricReader
{
    public async Task<IReadOnlyList<ProductionFabric>> ListAsync(CancellationToken cancellationToken) =>
        (await fabrics.ListAsync(cancellationToken)).Select(x => new ProductionFabric(x.Id, x.Code, x.Name, x.Price))
        .ToArray();
}

/// <summary>Active colours of «Кольори» for the workshop forms and their check.</summary>
internal sealed class ProductionColorReader(IFabricColorDirectory colors) : IProductionColorReader
{
    public async Task<IReadOnlyList<string>> ListActiveAsync(CancellationToken cancellationToken) =>
        (await colors.ListAsync(cancellationToken)).Where(x => x.IsActive).Select(x => x.Name).ToArray();
}

/// <summary>Reads the payroll coefficients from «Параметри ціноутворення» (Reference).</summary>
internal sealed class PayrollParameterReader(IPricingParameterDirectory parameters) : IPayrollParameterReader
{
    public async Task<IReadOnlyList<PayrollParameterValue>> ReadAsync(IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken) =>
        (await parameters.ListAsync(cancellationToken))
        .Where(x => codes.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
        .Select(x => new PayrollParameterValue(x.Code, x.Name, x.Value, x.Unit, x.UpdatedOn))
        .ToArray();
}
