namespace Identity.Application.Features.Users.SetStaffBlocked;

public sealed record SetStaffBlockedCommand(int UserId, bool Blocked) : ICommand<Result>;
