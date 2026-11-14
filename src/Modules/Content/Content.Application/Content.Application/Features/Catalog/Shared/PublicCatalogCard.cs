using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Shared;

/// <summary>
/// A product in a listing. <paramref name="PriceFrom"/> is the lowest large-wholesale price; null means «ціну
/// уточнюйте». <paramref name="Fabrics"/> are the names of the active fabrics of a sewing model, each once, in the
/// order of the model (R-13); empty for PPE.
/// </summary>
public sealed record PublicCatalogCard(
    string Slug,
    string Type,
    string? Code,
    string Name,
    int? SectionId,
    string? PhotoUrl,
    decimal? PriceFrom,
    IReadOnlyList<string> Fabrics);
