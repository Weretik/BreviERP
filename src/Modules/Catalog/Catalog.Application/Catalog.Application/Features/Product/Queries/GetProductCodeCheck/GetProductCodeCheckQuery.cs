namespace Catalog.Application.Features.Product.Queries.GetProductCodeCheck;

/// <summary>
/// Whether a code is free for a new product of the type (CAT-009). For sewing it also suggests the next free
/// MOD-XXX code; an empty <paramref name="Code"/> only asks for the suggestion.
/// </summary>
public sealed record GetProductCodeCheckQuery(string Type, string? Code) : IQuery<Result<ProductCodeCheck>>;

/// <param name="IsAvailable">Null when no code was given.</param>
public sealed record ProductCodeCheck(
    string? Code,
    bool? IsAvailable,
    int? TakenByProductId,
    string? TakenByProductName,
    string? SuggestedCode);
