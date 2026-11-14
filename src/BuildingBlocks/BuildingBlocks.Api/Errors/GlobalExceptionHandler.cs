using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Api.Errors;

/// <summary>
/// The one place that turns an exception into an HTTP answer. Business rules return typed results, so an exception here
/// is either a request that failed validation outside a <c>Result</c> response (400) or an unexpected failure (500).
/// The client gets a Ukrainian ProblemDetails with <c>traceId</c>; the exception text and stack trace stay in the log.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public const string ValidationTitle = "Перевірте дані.";
    public const string ValidationDetail = "Деякі поля заповнено неправильно.";
    public const string BadRequestTitle = "Неправильний запит.";
    public const string ServerErrorTitle = "Сталася помилка на сервері.";
    public const string ServerErrorDetail =
        "Спробуйте ще раз. Якщо помилка повториться, передайте адміністратору код traceId.";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        var problem = exception switch
        {
            ValidationException validation => Validation(validation),
            BadHttpRequestException badRequest => new ProblemDetails
            {
                Status = badRequest.StatusCode,
                Title = BadRequestTitle
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = ServerErrorTitle,
                Detail = ServerErrorDetail
            }
        };

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        problem.Extensions["traceId"] = traceId;
        if (problem.Status >= StatusCodes.Status500InternalServerError)
            Unhandled(logger, httpContext.Request.Method, httpContext.Request.Path.Value ?? string.Empty, traceId, exception);
        else
            Rejected(logger, httpContext.Request.Method, httpContext.Request.Path.Value ?? string.Empty, exception.GetType().Name);

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ProblemDetails Validation(ValidationException exception)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ValidationTitle,
            Detail = ValidationDetail
        };
        problem.Extensions["errors"] = exception.Errors
            .GroupBy(failure => failure.PropertyName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray(),
                StringComparer.Ordinal);
        return problem;
    }

    [LoggerMessage(EventId = 5000, Level = LogLevel.Error,
        Message = "Unhandled exception for {Method} {Path}, traceId {TraceId}")]
    private static partial void Unhandled(ILogger logger, string method, string path, string traceId, Exception exception);

    [LoggerMessage(EventId = 5001, Level = LogLevel.Warning,
        Message = "Request {Method} {Path} rejected by {ExceptionType}")]
    private static partial void Rejected(ILogger logger, string method, string path, string exceptionType);
}
