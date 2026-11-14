using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.DeleteContentArticle;

public sealed class DeleteContentArticleHandler(IContentDbContext db)
    : ICommandHandler<DeleteContentArticleCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteContentArticleCommand command, CancellationToken cancellationToken)
    {
        var article = await db.Articles.Include(x => x.Texts).FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (article is null) return Result.NotFound(ArticleMessages.NotFound);
        db.Articles.Remove(article);
        await db.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}
