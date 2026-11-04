namespace Production.Application.Contracts;

/// <summary>
/// A payroll coefficient read from «Параметри ціноутворення». <paramref name="Purpose"/> says what the workshop uses it
/// for; <paramref name="Problem"/> is set when the parameter is missing or has no value.
/// </summary>
public sealed record PayrollParameterResponse(
    string Code, string Purpose, string? Name, decimal? Value, string? Unit, DateOnly? UpdatedOn, string? Problem);
