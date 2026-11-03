using Catalog.Application.Features.Product.SheetImport.DTOs;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>Articles and composition lines of a sewing model in sheet order.</summary>
/// <param name="ReviewedAt">When the model is checked, every line is marked reviewed at this time; null leaves
/// the lines unreviewed.</param>
public sealed record SewingCompositionDraft(
    IReadOnlyList<SheetSewingArticleRow> Articles,
    IReadOnlyList<SewingMaterialDraft> Materials,
    IReadOnlyList<SewingOperationDraft> Operations,
    DateTimeOffset? ReviewedAt);
