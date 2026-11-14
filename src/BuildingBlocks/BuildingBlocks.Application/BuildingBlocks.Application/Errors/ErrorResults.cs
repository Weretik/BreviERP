using BuildingBlocks.Domain.Errors;
using BuildingBlocks.Domain.Results;
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
            ErrorType.Validation => Result.Invalid(error.ToValidationError()),
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
            ErrorType.Validation => Result<T>.Invalid(error.ToValidationError()),
            ErrorType.NotFound => Result<T>.NotFound(error.Message),
            ErrorType.Conflict => Result<T>.Conflict(error.Message),
            ErrorType.Forbidden => Result<T>.Forbidden(error.Message),
            _ => Result<T>.Error(error.Message)
        };
    }

    /// <summary>The validation error the admin shows under <see cref="Error.Field"/>, or as a general error.</summary>
    public static ValidationError ToValidationError(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new ValidationError(error.Field ?? error.Code, error.Message, error.Code, ValidationSeverity.Error);
    }

    public static Result ToResult(this DomainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? Result.Success() : result.Error!.ToResult();
    }

    /// <summary>The failure of <paramref name="result"/>, or null on success (steps that return a refusal or nothing).</summary>
    public static Result? FailureOrNull(this DomainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? null : result.Error!.ToResult();
    }

    /// <summary>The failure of <paramref name="result"/> as a result of another value type.</summary>
    public static Result<T> ToResult<T>(this DomainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsFailure ? result.Error!.ToResult<T>() : throw new InvalidOperationException("The result succeeded.");
    }

    /// <summary>The failure of <paramref name="result"/> as a result of another value type.</summary>
    public static Result<TOut> ToResult<TIn, TOut>(this DomainResult<TIn> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsFailure ? result.Error!.ToResult<TOut>() : throw new InvalidOperationException("The result succeeded.");
    }
}
