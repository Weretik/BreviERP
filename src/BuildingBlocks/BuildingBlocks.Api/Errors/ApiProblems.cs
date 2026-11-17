using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace BuildingBlocks.Api.Errors;

/// <summary>
/// Every error answer of the API is a Ukrainian <c>ProblemDetails</c> (RFC 9457): a refused input is 400 with
/// <c>errors</c> by field, a conflict is 409 with the reasons in <c>detail</c> and in the <c>errors</c> list.
/// </summary>
public static class ApiProblems
{
    public const string InvalidTitle = "Перевірте введені дані.";
    public const string ConflictTitle = "Дані вже змінилися або суперечать іншим.";
    public const string NotFoundTitle = "Не знайдено.";
    public const string ErrorTitle = "Не вдалося виконати запит.";

    /// <summary>400 with the messages grouped by field (the identifier of each error).</summary>
    public static IResult Invalid(IEnumerable<ValidationError> errors)
    {
        var list = errors.ToList();
        var byField = list
            .GroupBy(error => error.Identifier ?? "")
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
        return Results.ValidationProblem(byField, detail: list.FirstOrDefault()?.ErrorMessage, title: InvalidTitle);
    }

    /// <summary>400 for a request the endpoint refuses before any handler (a missing file, a bad query value).</summary>
    public static IResult BadRequest(string detail) =>
        Results.Problem(detail: detail, statusCode: StatusCodes.Status400BadRequest, title: InvalidTitle);

    public const string UnavailableTitle = "Зовнішній сервіс зараз недоступний.";
    public const string RefusedTitle = "Дію не виконано.";

    /// <summary>409 with the reasons joined in <c>detail</c> and kept as the <c>errors</c> list.</summary>
    public static IResult Conflict(IEnumerable<string> errors) =>
        WithErrors(StatusCodes.Status409Conflict, ConflictTitle, errors);

    /// <summary>503 when an outside service (Google, Telegram, OpenAI, the CDN) did not answer.</summary>
    public static IResult Unavailable(IEnumerable<string> errors) =>
        WithErrors(StatusCodes.Status503ServiceUnavailable, UnavailableTitle, errors);

    /// <summary>422 when an outside service answered but refused (a sync or a connection test).</summary>
    public static IResult Refused(IEnumerable<string> errors) =>
        WithErrors(StatusCodes.Status422UnprocessableEntity, RefusedTitle, errors);

    private static IResult WithErrors(int status, string title, IEnumerable<string> errors)
    {
        var list = errors.ToList();
        return Results.Problem(detail: list.Count == 0 ? null : string.Join(" ", list), statusCode: status,
            title: title, extensions: new Dictionary<string, object?> { ["errors"] = list });
    }

    public static IResult NotFound() =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: NotFoundTitle);

    public static IResult Error() =>
        Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: ErrorTitle);
}
