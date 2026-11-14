using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

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
