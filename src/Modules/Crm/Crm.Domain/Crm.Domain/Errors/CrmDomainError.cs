using BuildingBlocks.Domain.Abstractions;

namespace Crm.Domain.Errors;

public record CrmDomainError(string Code, string Message) : IDomainError
{
}
