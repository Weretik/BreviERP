using BuildingBlocks.Domain.Abstractions;

namespace Catalog.Domain.Errors;

public sealed record CatalogDomainError(string Code, string Message) : IDomainError;
