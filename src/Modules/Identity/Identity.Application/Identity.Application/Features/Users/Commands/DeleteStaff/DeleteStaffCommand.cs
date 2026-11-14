namespace Identity.Application.Features.Users.Commands.DeleteStaff;

public sealed record DeleteStaffCommand(int UserId) : ICommand<Result>;
