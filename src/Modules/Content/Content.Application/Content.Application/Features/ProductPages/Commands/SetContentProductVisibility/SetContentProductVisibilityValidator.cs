using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;

namespace Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;

public sealed class SetContentProductVisibilityValidator : AbstractValidator<SetContentProductVisibilityCommand>
{
    public SetContentProductVisibilityValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}
