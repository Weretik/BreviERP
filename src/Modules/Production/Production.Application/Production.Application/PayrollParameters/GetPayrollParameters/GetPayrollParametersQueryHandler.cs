using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.PayrollParameters.GetPayrollParameters;

public sealed class GetPayrollParametersQueryHandler(IPayrollParameterReader reader)
    : IQueryHandler<GetPayrollParametersQuery, Result<IReadOnlyList<PayrollParameterResponse>>>
{
    public const string Missing = "Параметра немає в «Параметри ціноутворення».";
    public const string NoValue = "У параметра не задано значення.";

    public async ValueTask<Result<IReadOnlyList<PayrollParameterResponse>>> Handle(GetPayrollParametersQuery query,
        CancellationToken cancellationToken)
    {
        var values = (await reader.ReadAsync(PayrollParameterCodes.All.Select(x => x.Code).ToArray(), cancellationToken))
            .ToDictionary(x => x.Code, StringComparer.Ordinal);
        IReadOnlyList<PayrollParameterResponse> rows = PayrollParameterCodes.All.Select(x =>
            values.TryGetValue(x.Code, out var value)
                ? new PayrollParameterResponse(x.Code, x.Purpose, value.Name, value.Value, value.Unit, value.UpdatedOn,
                    value.Value is null ? NoValue : null)
                : new PayrollParameterResponse(x.Code, x.Purpose, null, null, null, null, Missing)).ToList();
        return Result.Success(rows);
    }
}
