using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Features.Product.Shared.Sewing.Views;
namespace Catalog.Application.Abstractions.Sewing;

/// <summary>
/// One sewing model with its fabric variants and every material line: variant fabric lines carry
/// <see cref="SewingMaterialView.ArticleId"/>, shared lines do not.
/// </summary>
public sealed record SewingCompositionListRow(
    int ProductId,
    string? ModelCode,
    IReadOnlyList<SewingArticleView> Articles,
    IReadOnlyList<SewingMaterialView> Materials);
