using FluentValidation;
using Identity.Domain.Authorization;

namespace Identity.Application.Features.Access.Commands.RenameRole;

public sealed class RenameRoleValidator : AbstractValidator<RenameRoleCommand>
{
    public RenameRoleValidator()
    {
        RuleFor(x => x.Role).NotEmpty().MaximumLength(256).WithMessage("Оберіть роль.")
            .Must(role => !AccessCatalog.IsBuiltIn(role)).WithMessage("Вбудовану роль перейменувати не можна.");
        RuleFor(x => x.Title).NotEmpty().Must(title => title.Trim().Length is > 0 and <= 100)
            .WithMessage("Назва ролі — від 1 до 100 символів.");
    }
}
