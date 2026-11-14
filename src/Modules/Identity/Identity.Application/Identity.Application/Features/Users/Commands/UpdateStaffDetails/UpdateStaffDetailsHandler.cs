using Identity.Application.Abstractions;

namespace Identity.Application.Features.Users.Commands.UpdateStaffDetails;

public sealed class UpdateStaffDetailsHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<UpdateStaffDetailsCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateStaffDetailsCommand command, CancellationToken cancellationToken)
        => await staffAccountService.UpdateDetailsAsync(command.UserId, command.FullName, command.Email, command.PhoneNumber, cancellationToken);
}
