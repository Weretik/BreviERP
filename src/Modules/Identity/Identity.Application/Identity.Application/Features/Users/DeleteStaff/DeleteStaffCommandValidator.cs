using FluentValidation;

namespace Identity.Application.Features.Users.DeleteStaff;

public sealed class DeleteStaffCommandValidator : AbstractValidator<DeleteStaffCommand>
{
    public DeleteStaffCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
