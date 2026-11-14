using Content.Application.Abstractions;
using Content.Application.Articles.Shared;
using Content.Application.Contracts;
using Content.Application.Redirects.Shared;
using Content.Domain.Articles;
using Content.Domain.ProductPages;

namespace Content.Application.Articles.Save;

/// <summary>
/// Creates an article (<paramref name="Id"/> is null, <paramref name="Revision"/> 0) or replaces it as a whole
/// (CNT-004 R-03). An empty <paramref name="Slug"/> is built from the Ukrainian title. The cover and the photos of the
/// bodies must be in the «Фото» library.
/// </summary>
public sealed record SaveContentArticleCommand(
    int? Id,
    int Revision,
    string? Slug,
    DateOnly PublishedOn,
    bool ShowOnSite,
    int? CoverPhotoId,
    ArticleTextInput? Uk,
    ArticleTextInput? Ru,
    string UpdatedBy) : ICommand<Result<ArticleResponse>>;

public sealed class SaveContentArticleCommandValidator : AbstractValidator<SaveContentArticleCommand>
{
    public SaveContentArticleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id is not null);
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Slug).Must(slug => string.IsNullOrWhiteSpace(slug) || ProductPageRules.IsValidSlug(slug.Trim()))
            .WithMessage(ArticleMessages.SlugInvalid);
        RuleFor(x => x.PublishedOn).Must(date => date.Year is >= 2000 and <= 2100).WithMessage("Вкажіть дату статті.");
        RuleFor(x => x.CoverPhotoId).GreaterThan(0).When(x => x.CoverPhotoId is not null);
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Uk).Must(uk => ProductPageRules.Clean(uk?.Title) is not null)
            .WithMessage(ArticleMessages.TitleRequired);
        RuleFor(x => x.Uk!).SetValidator(new ArticleTextInputValidator("Українська")).When(x => x.Uk is not null);
        RuleFor(x => x.Ru!).SetValidator(new ArticleTextInputValidator("Російська")).When(x => x.Ru is not null);
    }
}

/// <summary>Shape limits of the texts of one language (CNT-004 R-01).</summary>
public sealed class ArticleTextInputValidator : AbstractValidator<ArticleTextInput>
{
    public ArticleTextInputValidator(string language)
    {
        RuleFor(x => x.Title).MaximumLength(ArticleRules.TitleMaxLength)
            .WithMessage($"{language}: назва — не більше {ArticleRules.TitleMaxLength} символів.");
        RuleFor(x => x.Excerpt).MaximumLength(ArticleRules.ExcerptMaxLength)
            .WithMessage($"{language}: короткий опис — не більше {ArticleRules.ExcerptMaxLength} символів.");
        RuleFor(x => x.SeoTitle).MaximumLength(ProductPageRules.SeoTitleMaxLength)
            .WithMessage($"{language}: SEO-заголовок — не більше {ProductPageRules.SeoTitleMaxLength} символів.");
        RuleFor(x => x.SeoDescription).MaximumLength(ProductPageRules.SeoDescriptionMaxLength)
            .WithMessage($"{language}: SEO-опис — не більше {ProductPageRules.SeoDescriptionMaxLength} символів.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Count <= ArticleRules.MaxBlocks)
            .WithMessage($"{language}: текст — не більше {ArticleRules.MaxBlocks} блоків.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.All(block =>
                block is not null && Enum.IsDefined(block.Kind)))
            .WithMessage($"{language}: невідомий тип блоку тексту.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Where(block => block is not null).All(block =>
                block.Kind != ArticleBlockKind.Image || block.PhotoId is > 0))
            .WithMessage($"{language}: у блоку з фото не обрано фото.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Where(block => block is not null).All(block =>
                (block.Alt?.Length ?? 0) <= ArticleRules.AltMaxLength))
            .WithMessage($"{language}: підпис фото — не більше {ArticleRules.AltMaxLength} символів.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Where(block => block is not null).Sum(block =>
                (block.Text?.Length ?? 0) + (block.Lead?.Length ?? 0) + (block.Items?.Sum(item => item?.Length ?? 0) ?? 0))
            <= ArticleRules.TextMaxLength)
            .WithMessage($"{language}: текст — не більше {ArticleRules.TextMaxLength} символів.");
    }
}

public sealed class SaveContentArticleCommandHandler(IContentDbContext db, IContentMediaReader media, TimeProvider clock)
    : ICommandHandler<SaveContentArticleCommand, Result<ArticleResponse>>
{
    public async ValueTask<Result<ArticleResponse>> Handle(SaveContentArticleCommand command,
        CancellationToken cancellationToken)
    {
        Article? article = null;
        if (command.Id is { } id)
        {
            article = await db.Articles.Include(x => x.Texts).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (article is null) return Result.NotFound(ArticleMessages.NotFound);
            if (article.Revision != command.Revision) return Result.Conflict(ArticleMessages.Stale);
        }
        else if (await db.Articles.CountAsync(cancellationToken) >= ArticleRules.MaxArticles)
        {
            return Result.Conflict(ArticleMessages.TooMany);
        }

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? article?.Slug ?? await ArticleReads.FreeSlugAsync(db, command.Uk?.Title, cancellationToken)
            : command.Slug.Trim();
        if (await ArticleReads.SlugTakenAsync(db, slug, article?.Id, cancellationToken))
            return Result.Conflict(ArticleMessages.SlugTaken);

        var now = clock.GetUtcNow();
        var articleOldSlug = article?.Slug;
        if (article is null)
        {
            var created = Article.Create(slug, command.PublishedOn, command.UpdatedBy, now);
            if (created.IsFailure) return created.Error!.ToResult<ArticleResponse>();
            article = created.Value;
            db.Articles.Add(article);
        }

        var texts = new Dictionary<string, ArticleTextInput>();
        if (command.Uk is not null) texts[ProductPageRules.Ukrainian] = command.Uk;
        if (command.Ru is not null) texts[ProductPageRules.Russian] = command.Ru;
        var replaced = article.Replace(slug, command.PublishedOn, command.ShowOnSite, command.CoverPhotoId, texts,
            command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<ArticleResponse>();

        // The old address keeps working: a 301 to the new one (CNT-005 R-05).
        if (articleOldSlug is not null && articleOldSlug != slug)
        {
            var redirected = await SiteRedirectWriter.SlugChangedAsync(db, SiteRedirectWriter.Articles, articleOldSlug, slug,
                command.UpdatedBy, now, cancellationToken);
            if (redirected.IsFailure) return redirected.Error!.ToResult<ArticleResponse>();
        }

        // Every photo must be in the library: a mistyped id would leave a hole on the site.
        var photos = await ArticleReads.PhotosAsync(media, article.PhotoIds(), cancellationToken);
        if (article.PhotoIds().Any(photoId => !photos.ContainsKey(photoId)))
            return Result.Conflict(ArticleMessages.PhotoNotFound);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(ArticleMessages.Stale);
        }

        return ArticleMapping.Detail(article, photos);
    }
}
