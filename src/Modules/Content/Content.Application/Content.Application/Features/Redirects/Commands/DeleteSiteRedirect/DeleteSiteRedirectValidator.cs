using Content.Application.Abstractions;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;

public sealed class DeleteSiteRedirectValidator : AbstractValidator<DeleteSiteRedirectCommand>
{
    public DeleteSiteRedirectValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
