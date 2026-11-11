using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.ProductPages.Shared;
using Content.Domain.ProductPages;

namespace Content.Application.ProductPages.Save;

/// <summary>
/// Creates the page of a product or replaces it as a whole (CNT-002 R-05). <paramref name="Revision"/> is the number
/// the editor showed: 0 for a product without a page. An empty <paramref name="Slug"/> is built from the name.
/// <paramref name="CategoryIds"/> replaces the site categories of the product (CNT-003 R-03); null keeps them.
/// </summary>
public sealed record SaveContentProductCommand(
    int ProductId,
    int Revision,
    string? Slug,
    bool ShowOnSite,
    ProductPageTextInput? Uk,
    ProductPageTextInput? Ru,
    string UpdatedBy,
    IReadOnlyList<int>? CategoryIds = null) : ICommand<Result<ContentProductResponse>>
{
    public const int MaxCategories = 50;
}

public sealed class SaveContentProductCommandValidator : AbstractValidator<SaveContentProductCommand>
{
    public SaveContentProductCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Slug).Must(slug => string.IsNullOrWhiteSpace(slug) || ProductPageRules.IsValidSlug(slug.Trim()))
            .WithMessage(ContentProductMessages.SlugInvalid);
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.CategoryIds).Must(ids => ids is null || (ids.Count <= SaveContentProductCommand.MaxCategories
                && ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count))
            .WithMessage($"Розділи сайту: не більше {SaveContentProductCommand.MaxCategories}, без повторів.");
        RuleFor(x => x.Uk!).SetValidator(new ProductPageTextInputValidator("Українська")).When(x => x.Uk is not null);
        RuleFor(x => x.Ru!).SetValidator(new ProductPageTextInputValidator("Російська")).When(x => x.Ru is not null);
    }
}

/// <summary>Shape limits of the texts of one language (CNT-002 R-04).</summary>
public sealed class ProductPageTextInputValidator : AbstractValidator<ProductPageTextInput>
{
    public ProductPageTextInputValidator(string language)
    {
        RuleFor(x => x.Name).MaximumLength(ProductPageRules.NameMaxLength)
            .WithMessage($"{language}: назва — не більше {ProductPageRules.NameMaxLength} символів.");
        RuleFor(x => x.SeoTitle).MaximumLength(ProductPageRules.SeoTitleMaxLength)
            .WithMessage($"{language}: SEO-заголовок — не більше {ProductPageRules.SeoTitleMaxLength} символів.");
        RuleFor(x => x.SeoDescription).MaximumLength(ProductPageRules.SeoDescriptionMaxLength)
            .WithMessage($"{language}: SEO-опис — не більше {ProductPageRules.SeoDescriptionMaxLength} символів.");

        RuleFor(x => x.Summary).Must(facts => facts is null || facts.Count <= ProductPageRules.MaxFacts)
            .WithMessage($"{language}: короткі відомості — не більше {ProductPageRules.MaxFacts} рядків.");
        RuleFor(x => x.Summary).Must(facts => facts is null || facts.All(fact =>
                fact is not null && (fact.Label?.Length ?? 0) <= ProductPageRules.FactLabelMaxLength
                                 && (fact.Text?.Length ?? 0) <= ProductPageRules.FactTextMaxLength))
            .WithMessage($"{language}: рядок коротких відомостей задовгий.");

        RuleFor(x => x.Description).Must(blocks => blocks is null || blocks.Count <= ProductPageRules.MaxBlocks)
            .WithMessage($"{language}: опис — не більше {ProductPageRules.MaxBlocks} блоків.");
        RuleFor(x => x.Description).Must(blocks => blocks is null || blocks.All(block =>
                block is not null && Enum.IsDefined(block.Kind)))
            .WithMessage($"{language}: невідомий тип блоку опису.");
        RuleFor(x => x.Description).Must(blocks => blocks is null || blocks.Where(block => block is not null).Sum(block =>
                (block.Text?.Length ?? 0) + (block.Lead?.Length ?? 0) + (block.Items?.Sum(item => item?.Length ?? 0) ?? 0))
            <= ProductPageRules.DescriptionMaxLength)
            .WithMessage($"{language}: опис — не більше {ProductPageRules.DescriptionMaxLength} символів.");

        RuleFor(x => x.Specs).Must(groups => groups is null || groups.Count <= ProductPageRules.MaxSpecGroups)
            .WithMessage($"{language}: характеристики — не більше {ProductPageRules.MaxSpecGroups} груп.");
        RuleFor(x => x.Specs).Must(groups => groups is null || groups.All(group =>
                group is not null && (group.Title?.Length ?? 0) <= ProductPageRules.SpecTitleMaxLength
                                  && (group.Rows?.Count ?? 0) <= ProductPageRules.MaxSpecRows
                                  && (group.Rows ?? []).All(row =>
                                      row is not null && (row.Name?.Length ?? 0) <= ProductPageRules.SpecNameMaxLength
                                                      && (row.Value?.Length ?? 0) <= ProductPageRules.SpecValueMaxLength)))
            .WithMessage($"{language}: група або рядок характеристик задовгі.");
    }
}

public sealed class SaveContentProductCommandHandler(IContentDbContext db, IContentCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<SaveContentProductCommand, Result<ContentProductResponse>>
{
    public async ValueTask<Result<ContentProductResponse>> Handle(SaveContentProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await catalog.GetAsync(command.ProductId, cancellationToken);
        if (product is null) return Result.NotFound(ContentProductMessages.ProductNotFound);

        var page = await db.ProductPages.Include(x => x.Texts)
            .FirstOrDefaultAsync(x => x.ProductId == command.ProductId, cancellationToken);
        if ((page?.Revision ?? 0) != command.Revision) return Result.Conflict(ContentProductMessages.Stale);

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? page?.Slug ?? await ProductPageSlugs.FreeAsync(db, product, cancellationToken)
            : command.Slug.Trim();
        if (await ProductPageSlugs.TakenAsync(db, slug, product.Id, cancellationToken))
            return Result.Conflict(ContentProductMessages.SlugTaken);

        if (command.CategoryIds is not null
            && !await ProductCategoryLinks.ReplaceAsync(db, product.Id, command.CategoryIds, cancellationToken))
            return Result.Conflict(ContentProductMessages.CategoryNotFound);

        var now = clock.GetUtcNow();
        if (page is null)
        {
            page = new ProductPage(product.Id, slug, command.UpdatedBy, now);
            db.ProductPages.Add(page);
        }

        var texts = new Dictionary<string, ProductPageTextInput>();
        if (command.Uk is not null) texts[ProductPageRules.Ukrainian] = command.Uk;
        if (command.Ru is not null) texts[ProductPageRules.Russian] = command.Ru;
        page.Replace(slug, command.ShowOnSite, texts, command.UpdatedBy, now);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(ContentProductMessages.Stale);
        }

        return ContentProductMapping.Detail(product, page, page.Slug,
            await ProductCategoryLinks.IdsAsync(db, product.Id, cancellationToken));
    }
}
