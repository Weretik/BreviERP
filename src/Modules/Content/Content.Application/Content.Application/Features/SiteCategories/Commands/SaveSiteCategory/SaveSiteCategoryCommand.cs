using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

/// <summary>
/// Creates a category (<paramref name="Id"/> is null, <paramref name="Revision"/> 0) or replaces it as a whole
/// (CNT-003 R-04). <paramref name="ParentId"/> is null for a category and the category id for a subcategory; changing
/// it moves the category to the end of the new parent. An empty <paramref name="Slug"/> is built from the Ukrainian name.
/// </summary>
public sealed record SaveSiteCategoryCommand(
    int? Id,
    int Revision,
    int? ParentId,
    string? Slug,
    bool ShowOnSite,
    SiteCategoryTextInput? Uk,
    SiteCategoryTextInput? Ru,
    string UpdatedBy) : ICommand<Result<SiteCategoryResponse>>;

public sealed class SaveSiteCategoryValidator : AbstractValidator<SaveSiteCategoryCommand>
{
    public SaveSiteCategoryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id is not null);
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId is not null);
        RuleFor(x => x.ParentId).Must((command, parentId) => parentId is null || parentId != command.Id)
            .WithMessage(SiteCategoryMessages.ParentIsSubcategory);
        RuleFor(x => x.Slug).Must(slug => string.IsNullOrWhiteSpace(slug) || ProductPageRules.IsValidSlug(slug.Trim()))
            .WithMessage(SiteCategoryMessages.SlugInvalid);
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Uk).Must(uk => ProductPageRules.Clean(uk?.Name) is not null)
            .WithMessage(SiteCategoryMessages.NameRequired);
        RuleFor(x => x.Uk!).SetValidator(new SiteCategoryTextInputValidator("Українська")).When(x => x.Uk is not null);
        RuleFor(x => x.Ru!).SetValidator(new SiteCategoryTextInputValidator("Російська")).When(x => x.Ru is not null);
    }
}

/// <summary>Shape limits of the texts of one language (CNT-003 R-01).</summary>
public sealed class SiteCategoryTextInputValidator : AbstractValidator<SiteCategoryTextInput>
{
    public SiteCategoryTextInputValidator(string language)
    {
        RuleFor(x => x.Name).MaximumLength(SiteCategoryRules.NameMaxLength)
            .WithMessage($"{language}: назва — не більше {SiteCategoryRules.NameMaxLength} символів.");
        RuleFor(x => x.SeoTitle).MaximumLength(ProductPageRules.SeoTitleMaxLength)
            .WithMessage($"{language}: SEO-заголовок — не більше {ProductPageRules.SeoTitleMaxLength} символів.");
        RuleFor(x => x.SeoDescription).MaximumLength(ProductPageRules.SeoDescriptionMaxLength)
            .WithMessage($"{language}: SEO-опис — не більше {ProductPageRules.SeoDescriptionMaxLength} символів.");
        RuleFor(x => x.Text).Must(blocks => blocks is null || blocks.Count <= SiteCategoryRules.MaxBlocks)
            .WithMessage($"{language}: текст — не більше {SiteCategoryRules.MaxBlocks} блоків.");
        RuleFor(x => x.Text).Must(blocks => blocks is null || blocks.All(block =>
                block is not null && Enum.IsDefined(block.Kind)))
            .WithMessage($"{language}: невідомий тип блоку тексту.");
        RuleFor(x => x.Text).Must(blocks => blocks is null || blocks.Where(block => block is not null).Sum(block =>
                (block.Text?.Length ?? 0) + (block.Lead?.Length ?? 0) + (block.Items?.Sum(item => item?.Length ?? 0) ?? 0))
            <= SiteCategoryRules.TextMaxLength)
            .WithMessage($"{language}: текст — не більше {SiteCategoryRules.TextMaxLength} символів.");
    }
}

public sealed class SaveSiteCategoryHandler(IContentDbContext db, TimeProvider clock)
    : ICommandHandler<SaveSiteCategoryCommand, Result<SiteCategoryResponse>>
{
    public async ValueTask<Result<SiteCategoryResponse>> Handle(SaveSiteCategoryCommand command,
        CancellationToken cancellationToken)
    {
        SiteCategory? category = null;
        if (command.Id is { } id)
        {
            category = await db.SiteCategories.Include(x => x.Texts).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (category is null) return Result.NotFound(SiteCategoryMessages.NotFound);
            if (category.Revision != command.Revision) return Result.Conflict(SiteCategoryMessages.Stale);
        }
        else if (await db.SiteCategories.CountAsync(cancellationToken) >= SiteCategoryRules.MaxCategories)
        {
            return Result.Conflict(SiteCategoryMessages.TooMany);
        }

        if (command.ParentId is { } parentId)
        {
            var parent = await db.SiteCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == parentId, cancellationToken);
            if (parent is null) return Result.Conflict(SiteCategoryMessages.ParentNotFound);
            if (parent.ParentId is not null) return Result.Conflict(SiteCategoryMessages.ParentIsSubcategory);
            if (category is not null && await db.SiteCategories.AnyAsync(x => x.ParentId == category.Id, cancellationToken))
                return Result.Conflict(SiteCategoryMessages.HasSubcategories);
        }

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? category?.Slug ?? await SiteCategoryReads.FreeSlugAsync(db, command.Uk?.Name, cancellationToken)
            : command.Slug.Trim();
        if (await SiteCategoryReads.SlugTakenAsync(db, slug, category?.Id, cancellationToken))
            return Result.Conflict(SiteCategoryMessages.SlugTaken);

        var now = clock.GetUtcNow();
        var categoryOldSlug = category?.Slug;
        var sortOrder = category is not null && category.ParentId == command.ParentId
            ? category.SortOrder
            : await SiteCategoryReads.NextSortOrderAsync(db, command.ParentId, cancellationToken);
        if (category is null)
        {
            var created = SiteCategory.Create(command.ParentId, slug, sortOrder, command.UpdatedBy, now);
            if (created.IsFailure) return created.Error!.ToResult<SiteCategoryResponse>();
            category = created.Value;
            db.SiteCategories.Add(category);
        }

        var texts = new Dictionary<string, SiteCategoryTextInput>();
        if (command.Uk is not null) texts[ProductPageRules.Ukrainian] = command.Uk;
        if (command.Ru is not null) texts[ProductPageRules.Russian] = command.Ru;
        var replaced = category.Replace(command.ParentId, sortOrder, slug, command.ShowOnSite, texts, command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<SiteCategoryResponse>();

        // The old address keeps working: a 301 to the new one (CNT-005 R-05).
        if (categoryOldSlug is not null && categoryOldSlug != slug)
        {
            var redirected = await SiteRedirectWriter.SlugChangedAsync(db, SiteRedirectWriter.Categories, categoryOldSlug, slug,
                command.UpdatedBy, now, cancellationToken);
            if (redirected.IsFailure) return redirected.Error!.ToResult<SiteCategoryResponse>();
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(SiteCategoryMessages.Stale);
        }

        return await SiteCategoryReads.DetailAsync(db, category, cancellationToken);
    }
}
