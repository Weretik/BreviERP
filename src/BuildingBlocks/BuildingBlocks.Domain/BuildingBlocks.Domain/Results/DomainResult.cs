using BuildingBlocks.Domain.Errors;

namespace BuildingBlocks.Domain.Results;

/// <summary>
/// The outcome of a domain operation: success, or the <see cref="Errors.Error"/> a business rule reported. Business rules
/// return it; exceptions stay for programming errors only (module rules «Errors»).
/// </summary>
public sealed class DomainResult
{
    private static readonly DomainResult Ok = new(null);

    private DomainResult(Error? error) => Error = error;

    /// <summary>The error of a failure; null on success.</summary>
    public Error? Error { get; }

    public bool IsSuccess => Error is null;
    public bool IsFailure => Error is not null;

    public static DomainResult Success() => Ok;

    public static DomainResult Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator DomainResult(Error error) => Failure(error);

    /// <summary>Success when the condition holds, otherwise the error.</summary>
    public static DomainResult Ensure(bool condition, Func<Error> error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return condition ? Ok : Failure(error());
    }

    /// <summary>The first failure of the checks, in order, or success.</summary>
    public static DomainResult Combine(params DomainResult[] results)
        => results.FirstOrDefault(result => result.IsFailure) ?? Ok;

    /// <summary>
    /// For a change the caller already checked (an import applies rows it validated in its plan): a failure here is a
    /// programming error, like reading <see cref="DomainResult{T}.Value"/> of a failure.
    /// </summary>
    public void ThrowIfFailure()
    {
        if (Error is not null)
            throw new InvalidOperationException($"A checked change failed: {Error.Code}.");
    }
}

/// <summary>The outcome of a domain operation that produces a value.</summary>
public sealed class DomainResult<T>
{
    private readonly T? _value;

    private DomainResult(T? value, Error? error)
    {
        _value = value;
        Error = error;
    }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;
    public bool IsFailure => Error is not null;

    /// <summary>The value of a success; reading it from a failure is a programming error.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"No value: the operation failed with {Error!.Code}.");

    public static DomainResult<T> Success(T value) => new(value, null);

    public static DomainResult<T> Failure(Error error) => new(default, error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator DomainResult<T>(T value) => Success(value);

    public static implicit operator DomainResult<T>(Error error) => Failure(error);

    /// <summary>The plain outcome without the value.</summary>
    public DomainResult WithoutValue() => Error is null ? DomainResult.Success() : DomainResult.Failure(Error);

    /// <summary>The value turned by <paramref name="map"/> on success; the same error on failure.</summary>
    public DomainResult<TOut> Map<TOut>(Func<T, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return Error is null ? DomainResult<TOut>.Success(map(_value!)) : DomainResult<TOut>.Failure(Error);
    }
}
