using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;

namespace Crm.Application.Features.SiteRequests.Commands.ChangeSiteRequest;

public sealed class ChangeSiteRequestValidator : AbstractValidator<ChangeSiteRequestCommand>
{
    public ChangeSiteRequestValidator()
    {
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Action).Must(x => x is "Take" or "Done" or "Spam" or "Reopen")
            .WithMessage(SiteRequestMessages.Action).When(x => x.Input is not null);
        RuleFor(x => x.Input.Note).MaximumLength(SiteRequest.NoteMaxLength).WithMessage(SiteRequestMessages.Note)
            .When(x => x.Input is not null);
    }
}
