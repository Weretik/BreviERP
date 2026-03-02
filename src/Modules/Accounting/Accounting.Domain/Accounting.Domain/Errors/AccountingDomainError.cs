using BuildingBlocks.Domain.Abstractions;

namespace Accounting.Domain.Errors;

public record AccountingDomainError(string Code, string Message) : IDomainError
{
}
