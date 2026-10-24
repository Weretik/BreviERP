namespace Catalog.Application.Features.Product.Sewing;

public sealed record ListSewingArticlesQuery(int ProductId) : IQuery<Result<SewingArticleList>>;
public sealed record GetSewingCompositionQuery(int ProductId, long? ArticleId) : IQuery<Result<SewingComposition>>;
public sealed record CreateSewingArticleCommand(int ProductId, CreateSewingArticleRequest Request)
    : ICommand<Result<SewingArticleView>>;
public sealed record ReplaceSewingCompositionCommand(int ProductId, ReplaceSewingCompositionRequest Request, string Actor)
    : ICommand<Result<SewingComposition>>;
public sealed record ReplaceSewingArticleCompositionCommand(int ProductId, long ArticleId,
    ReplaceSewingArticleCompositionRequest Request, string Actor) : ICommand<Result<SewingComposition>>;
