namespace Identity.Application.Features.Users.Commands.SetStaffPassword;

public sealed record SetStaffPasswordCommand(int UserId, string Password) : ICommand<Result>;
