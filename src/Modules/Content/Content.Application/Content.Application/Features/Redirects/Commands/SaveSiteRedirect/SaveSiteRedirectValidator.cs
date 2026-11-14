using Content.Application.Abstractions;
using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.SaveSiteRedirect;

public sealed class SaveSiteRedirectValidator : AbstractValidator<SaveSiteRedirectCommand>
{
    public SaveSiteRedirectValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id is not null);
        RuleFor(x => x.FromPath).Must(path => SiteRedirectRules.Normalize(path) is not null)
            .WithMessage(RedirectMessages.FromInvalid);
        RuleFor(x => x.ToPath).Must(path => SiteRedirectRules.IsSitePath(SiteRedirectRules.Normalize(path)))
            .WithMessage(RedirectMessages.ToInvalid);
        RuleFor(x => x).Must(x => SiteRedirectRules.Normalize(x.FromPath) != SiteRedirectRules.Normalize(x.ToPath))
            .WithMessage(RedirectMessages.SameAddress).OverridePropertyName("ToPath");
        RuleFor(x => x.Note).MaximumLength(SiteRedirectRules.NoteMaxLength).WithMessage(RedirectMessages.NoteTooLong);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}
