using Production.Application.Abstractions;
using Production.Domain.Payroll;

namespace Production.Application.Features.Payroll.Shared;

internal static class PayrollInputRules
{
    public static bool IsKopecks(decimal value) => decimal.Round(value, 2) == value;

    public static bool IsReason(string? value) =>
        value?.Trim() is { Length: >= 3 and <= PayrollPayment.ReasonMaxLength };
}
