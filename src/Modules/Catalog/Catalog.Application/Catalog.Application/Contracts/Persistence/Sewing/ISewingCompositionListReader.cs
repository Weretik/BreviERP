namespace Catalog.Application.Features.Product.Sewing;

/// <summary>Reads the composition of every sewing model at once, for the workshop «Комплектація моделей».</summary>
public interface ISewingCompositionListReader
{
    Task<IReadOnlyList<SewingCompositionListRow>> ReadAllAsync(CancellationToken cancellationToken);
}

/// <summary>
/// One sewing model with its fabric variants and every material line: variant fabric lines carry
/// <see cref="SewingMaterialView.ArticleId"/>, shared lines do not.
/// </summary>
public sealed record SewingCompositionListRow(
    int ProductId,
    string? ModelCode,
    IReadOnlyList<SewingArticleView> Articles,
    IReadOnlyList<SewingMaterialView> Materials);
