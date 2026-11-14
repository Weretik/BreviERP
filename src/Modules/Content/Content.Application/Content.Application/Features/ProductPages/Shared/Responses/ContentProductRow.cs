using Content.Domain.ProductPages;

namespace Content.Application.Features.ProductPages.Shared.Responses;

/// <summary>
/// A row of «Контент → Товари». <paramref name="HasPage"/> is false when nobody has made the page yet. The
/// <c>Has…</c> flags say what the site would miss; <paramref name="OnSite"/> is true only when the mark is set and the
/// Catalog product is active. <paramref name="CategoryCount"/> is the number of site categories the product is in and
/// <paramref name="CategoryIds"/> names them.
/// </summary>
public sealed record ContentProductRow(
    int ProductId,
    string Type,
    string? Code,
    string Name,
    int? SectionId,
    string? SectionName,
    bool IsActive,
    string? MainPhotoUrl,
    bool HasPrice,
    bool HasPage,
    bool ShowOnSite,
    bool OnSite,
    string? Slug,
    bool HasUkrainianText,
    bool HasRussianText,
    bool HasSeo,
    DateTimeOffset? UpdatedAtUtc,
    int CategoryCount,
    IReadOnlyList<int> CategoryIds);
