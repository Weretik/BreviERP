using Catalog.Application.Features.Product.SheetImport.DTOs;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>Articles and composition lines of a sewing model in sheet order.</summary>
public sealed record SewingCompositionDraft(
    IReadOnlyList<SheetSewingArticleRow> Articles,
    IReadOnlyList<SewingMaterialDraft> Materials,
    IReadOnlyList<SewingOperationDraft> Operations);
