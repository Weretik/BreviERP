namespace BuildingBlocks.Domain.Errors;

/// <summary>
/// The one error type of every module: a stable <paramref name="Code"/> such as <c>Catalog.Product.Name.Required</c>,
/// a Ukrainian <paramref name="Message"/> shown to the user as is, and its <paramref name="Type"/>. Each module keeps its
/// errors in static <c>&lt;Aggregate&gt;Errors</c> classes. Domain and Application never mention HTTP.
/// <paramref name="Field"/> names the input the admin shows the message under (camelCase, as in the request); without
/// it the message is general.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Validation, string? Field = null)
{
    public static Error Validation(string code, string message, string? field = null) =>
        new(code, message, ErrorType.Validation, field);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error Unexpected(string code, string message) => new(code, message, ErrorType.Unexpected);
}
