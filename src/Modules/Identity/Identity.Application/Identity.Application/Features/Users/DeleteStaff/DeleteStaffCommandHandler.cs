using Identity.Application.Services;

namespace Identity.Application.Features.Users.DeleteStaff;

public sealed class DeleteStaffCommandHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<DeleteStaffCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteStaffCommand command, CancellationToken cancellationToken)
        => await staffAccountService.DeleteAsync(command.UserId, cancellationToken);
}
