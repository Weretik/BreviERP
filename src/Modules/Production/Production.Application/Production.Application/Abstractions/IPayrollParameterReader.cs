namespace Production.Application.Abstractions;

/// <summary>
/// Values of «Параметри ціноутворення» (Reference) by code. The workshop pays by the same coefficients the price is
/// built from, so it never keeps its own copy of them.
/// </summary>
public interface IPayrollParameterReader
{
    Task<IReadOnlyList<PayrollParameterValue>> ReadAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken);
}

public sealed record PayrollParameterValue(string Code, string Name, decimal? Value, string Unit, DateOnly? UpdatedOn);
