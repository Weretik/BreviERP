namespace Identity.Application.Features.Users.SetStaffPassword;

public sealed record SetStaffPasswordCommand(int UserId, string Password) : ICommand<Result>;
