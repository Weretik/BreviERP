using BuildingBlocks.Domain.Errors;
using ErrorType = BuildingBlocks.Domain.Errors.ErrorType;

namespace BuildingBlocks.Application.Errors;

/// <summary>
/// Turns a domain <see cref="Error"/> into the Application result. The only place that knows how each
/// <see cref="ErrorType"/> is carried by <c>Ardalis.Result</c>; the Api turns the result into HTTP.
/// </summary>
public static class ErrorResults
{
    public static Result ToResult(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.Type switch
        {
            ErrorType.Validation => Result.Invalid(new ValidationError(error.Code, error.Message)),
            ErrorType.NotFound => Result.NotFound(error.Message),
            ErrorType.Conflict => Result.Conflict(error.Message),
            ErrorType.Forbidden => Result.Forbidden(error.Message),
            _ => Result.Error(error.Message)
        };
    }

    public static Result<T> ToResult<T>(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.Type switch
        {
            ErrorType.Validation => Result<T>.Invalid(new ValidationError(error.Code, error.Message)),
            ErrorType.NotFound => Result<T>.NotFound(error.Message),
            ErrorType.Conflict => Result<T>.Conflict(error.Message),
            ErrorType.Forbidden => Result<T>.Forbidden(error.Message),
            _ => Result<T>.Error(error.Message)
        };
    }

    /// <summary>The same mapping for a response type known only at run time (Mediator pipeline behaviours).</summary>
    public static bool TryToResult<TResponse>(this Error error, out TResponse response)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (typeof(TResponse) == typeof(Result))
        {
            response = (TResponse)(object)error.ToResult();
            return true;
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var method = typeof(ErrorResults).GetMethods()
                .Single(m => m.Name == nameof(ToResult) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);
            response = (TResponse)method.Invoke(null, [error])!;
            return true;
        }

        response = default!;
        return false;
    }
}
