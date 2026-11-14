using Identity.Application.Abstractions;

namespace Identity.Application.Features.Users.Commands.DeleteStaff;

public sealed class DeleteStaffHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<DeleteStaffCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteStaffCommand command, CancellationToken cancellationToken)
        => await staffAccountService.DeleteAsync(command.UserId, cancellationToken);
}
