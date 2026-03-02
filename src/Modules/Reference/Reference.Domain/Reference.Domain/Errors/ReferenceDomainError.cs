using BuildingBlocks.Domain.Abstractions;

namespace Reference.Domain.Errors;

public record ReferenceDomainError(string Code, string Message) : IDomainError
{
}
