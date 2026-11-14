using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Features.Product.Queries.ListSewingArticles;

public sealed record SewingArticleList(int ProductId, string? ModelCode, long Version, IReadOnlyList<SewingArticleView> Articles);

