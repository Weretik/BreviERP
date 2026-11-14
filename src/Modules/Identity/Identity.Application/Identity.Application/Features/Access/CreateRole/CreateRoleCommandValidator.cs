using FluentValidation;

namespace Identity.Application.Features.Access.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Must(title => title.Trim().Length is > 0 and <= 100)
            .WithMessage("Назва ролі — від 1 до 100 символів.");
    }
}
