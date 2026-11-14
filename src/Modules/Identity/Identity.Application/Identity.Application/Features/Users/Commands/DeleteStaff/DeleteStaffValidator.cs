using FluentValidation;

namespace Identity.Application.Features.Users.Commands.DeleteStaff;

public sealed class DeleteStaffValidator : AbstractValidator<DeleteStaffCommand>
{
    public DeleteStaffValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
