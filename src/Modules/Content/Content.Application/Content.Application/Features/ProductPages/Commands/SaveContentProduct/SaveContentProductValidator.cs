using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.ProductPages.Commands.SaveContentProduct;

public sealed class SaveContentProductValidator : AbstractValidator<SaveContentProductCommand>
{
    public SaveContentProductValidator()
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
