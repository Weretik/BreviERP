using Content.Application.Abstractions;
using Content.Application.Articles.Shared;

namespace Content.Application.Articles.Delete;

/// <summary>Removes an article with its texts (CNT-004 R-03); its photos stay in the «Фото» library.</summary>
public sealed record DeleteContentArticleCommand(int Id) : ICommand<Result>;

public sealed class DeleteContentArticleCommandValidator : AbstractValidator<DeleteContentArticleCommand>
{
    public DeleteContentArticleCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class DeleteContentArticleCommandHandler(IContentDbContext db)
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
