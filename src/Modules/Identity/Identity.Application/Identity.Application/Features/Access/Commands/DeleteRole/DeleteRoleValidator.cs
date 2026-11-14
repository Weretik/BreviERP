using FluentValidation;
using Identity.Domain.Authorization;

namespace Identity.Application.Features.Access.Commands.DeleteRole;

public sealed class DeleteRoleValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleValidator()
    {
        RuleFor(x => x.Role).NotEmpty().MaximumLength(256)
            .Must(role => !AccessCatalog.IsBuiltIn(role)).WithMessage("Вбудовану роль видалити не можна.");
    }
}
