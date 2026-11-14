namespace Identity.Application.Features.Users.Commands.SetStaffBlocked;

public sealed record SetStaffBlockedCommand(int UserId, bool Blocked) : ICommand<Result>;
