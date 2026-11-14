namespace Identity.Application.Features.Users.Commands.ChangeStaffRole;

public sealed record ChangeStaffRoleCommand(int UserId, string Role) : ICommand<Result>;
