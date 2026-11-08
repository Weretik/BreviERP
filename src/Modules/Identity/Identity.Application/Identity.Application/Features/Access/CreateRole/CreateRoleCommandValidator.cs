using FluentValidation;

namespace Identity.Application.Features.Access.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Must(title => title.Trim().Length is > 0 and <= 100)
            .WithMessage("The role name must have 1 to 100 characters.");
    }
}
