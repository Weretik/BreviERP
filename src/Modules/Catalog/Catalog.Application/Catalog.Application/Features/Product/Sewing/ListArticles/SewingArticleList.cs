namespace Catalog.Application.Features.Product.Sewing;

public sealed record SewingArticleList(int ProductId, string? ModelCode, long Version, IReadOnlyList<SewingArticleView> Articles);

