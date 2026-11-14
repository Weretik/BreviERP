using FluentValidation;
using Identity.Domain.Authorization;

namespace Identity.Application.Features.Access.DeleteRole;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Role).NotEmpty().MaximumLength(256)
            .Must(role => !AccessCatalog.IsBuiltIn(role)).WithMessage("Вбудовану роль видалити не можна.");
    }
}
