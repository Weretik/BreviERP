namespace Identity.Application.Features.Users.DeleteStaff;

public sealed record DeleteStaffCommand(int UserId) : ICommand<Result>;
