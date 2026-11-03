namespace Identity.Application.Features.Users.ChangeStaffRole;

public sealed record ChangeStaffRoleCommand(int UserId, string Role) : ICommand<Result>;
