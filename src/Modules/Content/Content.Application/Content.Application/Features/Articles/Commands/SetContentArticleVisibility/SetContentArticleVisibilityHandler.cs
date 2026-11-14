using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.SetContentArticleVisibility;

public sealed class SetContentArticleVisibilityHandler(IContentDbContext db, TimeProvider clock)
    : ICommandHandler<SetContentArticleVisibilityCommand, Result<ArticleVisibilityResponse>>
{
    public async ValueTask<Result<ArticleVisibilityResponse>> Handle(SetContentArticleVisibilityCommand command,
        CancellationToken cancellationToken)
    {
        var article = await db.Articles.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (article is null) return Result.NotFound(ArticleMessages.NotFound);

        article.SetVisibility(command.ShowOnSite, command.UpdatedBy, clock.GetUtcNow());
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(ArticleMessages.Stale);
        }

        return new ArticleVisibilityResponse(article.Id, article.ShowOnSite, article.Revision);
    }
}
