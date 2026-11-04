using Identity.Application.Services;

namespace Identity.Application.Features.Users.UpdateStaffDetails;

public sealed class UpdateStaffDetailsCommandHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<UpdateStaffDetailsCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateStaffDetailsCommand command, CancellationToken cancellationToken)
        => await staffAccountService.UpdateDetailsAsync(command.UserId, command.FullName, command.Email, command.PhoneNumber, cancellationToken);
}
