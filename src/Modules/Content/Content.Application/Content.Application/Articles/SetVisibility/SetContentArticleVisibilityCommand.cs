using Content.Application.Abstractions;
using Content.Application.Articles.Shared;
using Content.Application.Contracts;

namespace Content.Application.Articles.SetVisibility;

/// <summary>«Показувати на сайті» from the list (CNT-004 SC-03).</summary>
public sealed record SetContentArticleVisibilityCommand(int Id, bool ShowOnSite, string UpdatedBy)
    : ICommand<Result<ArticleVisibilityResponse>>;

public sealed class SetContentArticleVisibilityCommandValidator : AbstractValidator<SetContentArticleVisibilityCommand>
{
    public SetContentArticleVisibilityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}

public sealed class SetContentArticleVisibilityCommandHandler(IContentDbContext db, TimeProvider clock)
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
