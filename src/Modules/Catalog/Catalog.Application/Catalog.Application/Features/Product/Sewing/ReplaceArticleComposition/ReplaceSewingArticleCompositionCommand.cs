namespace Catalog.Application.Features.Product.Sewing;

public sealed record ReplaceSewingArticleCompositionCommand(int ProductId, long ArticleId, ReplaceSewingArticleCompositionRequest Request, string Actor) : ICommand<Result<SewingComposition>>;

