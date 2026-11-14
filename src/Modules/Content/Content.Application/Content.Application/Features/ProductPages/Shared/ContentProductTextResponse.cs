using Content.Domain.ProductPages;

namespace Content.Application.Features.ProductPages.Shared;

/// <summary>Texts of one language; empty lists and nulls when nothing is written.</summary>
public sealed record ContentProductTextResponse(
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductFact> Summary,
    IReadOnlyList<ProductTextBlock> Description,
    IReadOnlyList<ProductSpecGroup> Specs);
