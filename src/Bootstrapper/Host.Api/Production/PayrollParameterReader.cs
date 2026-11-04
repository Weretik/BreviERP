using Mediator;
using Production.Application.Abstractions;
using Reference.Application.Features.PricingParameter.GetList;

namespace Host.Api.Production;

/// <summary>Reads the payroll coefficients from «Параметри ціноутворення» (Reference).</summary>
internal sealed class PayrollParameterReader(ISender sender) : IPayrollParameterReader
{
    public async Task<IReadOnlyList<PayrollParameterValue>> ReadAsync(IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPricingParametersQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Pricing parameters could not be read.");
        return result.Value
            .Where(x => codes.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
            .Select(x => new PayrollParameterValue(x.Code, x.Name, x.Value, x.Unit, x.UpdatedOn))
            .ToArray();
    }
}
