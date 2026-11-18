using Content.Application.Abstractions;

namespace Content.Application.Features.Catalog.Shared.Snapshot;

/// <summary>A product that the site shows: the Catalog product is active and its page is marked «Показувати на сайті».</summary>
public sealed record PublicCatalogEntry(ContentCatalogProduct Product, PublicProductPageView Page);

/// <param name="Sections">Active sections with at least one product on the site, in the order of the reference.</param>
/// <param name="Categories">Shown site categories in tree order: a category, then its subcategories.</param>
[System.ComponentModel.ImmutableObject(true)]
public sealed record PublicCatalogSnapshot(
    IReadOnlyList<PublicCatalogEntry> Products,
    IReadOnlyList<PublicCatalogSnapshotSection> Sections,
    IReadOnlyList<PublicCatalogSnapshotCategory> Categories);

/// <param name="Products">
/// Products on the site in the order of the category; a top-level category also has the products of its shown
/// subcategories after its own, each once (CNT-003 R-03).
/// </param>
/// <param name="OwnProductIds">Products put into this very category.</param>
public sealed record PublicCatalogSnapshotCategory(
    PublicSiteCategoryView Category,
    IReadOnlyList<PublicCatalogEntry> Products,
    IReadOnlySet<int> OwnProductIds);

public sealed record PublicCatalogSnapshotSection(int Id, string Type, string Name, string Slug, int ProductCount);
