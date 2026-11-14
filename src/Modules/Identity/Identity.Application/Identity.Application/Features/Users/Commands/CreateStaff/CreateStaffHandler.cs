using Identity.Application.Features.Users.Shared;
using Identity.Application.Abstractions;

namespace Identity.Application.Features.Users.Commands.CreateStaff;

public sealed class CreateStaffHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<CreateStaffCommand, Result<StaffAccountDto>>
{
    public async ValueTask<Result<StaffAccountDto>> Handle(CreateStaffCommand command, CancellationToken cancellationToken)
        => await staffAccountService.CreateAsync(command.Email.Trim(), command.FullName, command.Role, command.Password, command.PhoneNumber, cancellationToken);
}
