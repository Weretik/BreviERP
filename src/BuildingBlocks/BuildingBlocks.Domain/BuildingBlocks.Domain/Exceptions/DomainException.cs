using BuildingBlocks.Domain.Errors;

namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// A broken business rule thrown by the Domain. Being replaced by returned results (module rules «Errors»); until then the
/// Mediator pipeline maps it by <see cref="Error.Type"/>.
/// </summary>
public sealed class DomainException(Error error) : Exception(error.Message)
{
    public Error Error { get; } = error;
}
