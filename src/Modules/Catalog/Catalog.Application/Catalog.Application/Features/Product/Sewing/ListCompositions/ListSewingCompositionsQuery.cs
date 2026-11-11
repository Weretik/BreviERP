namespace Catalog.Application.Features.Product.Sewing;

/// <summary>The composition of every sewing model with reference codes and names (no prices).</summary>
public sealed record ListSewingCompositionsQuery : IQuery<Result<IReadOnlyList<SewingModelCompositionSummary>>>;

/// <summary>A sewing model and its material lines: variant fabrics first, then the shared lines in card order.</summary>
public sealed record SewingModelCompositionSummary(
    int ProductId,
    string? ModelCode,
    IReadOnlyList<SewingCompositionSummaryLine> Lines);

/// <param name="Kind">Fabric, Accessory or OtherCost.</param>
/// <param name="Article">The sale SKU of the fabric variant; null for a line shared by the whole model.</param>
/// <param name="Code">Reference code (FAB-…, FUR-…) or the cost code; null when unknown.</param>
/// <param name="Name">Reference name; null when the reference is gone.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
/// <param name="ArticleId">The fabric variant (article) id; null for a line shared by the whole model.</param>
public sealed record SewingCompositionSummaryLine(
    string Kind,
    string? Article,
    string? Code,
    string? Name,
    decimal Quantity,
    string Unit,
    string? Comment,
    long? ArticleId = null);
