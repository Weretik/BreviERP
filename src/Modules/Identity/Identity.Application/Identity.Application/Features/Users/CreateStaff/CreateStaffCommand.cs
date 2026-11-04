using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.CreateStaff;

public sealed record CreateStaffCommand(string Email, string FullName, string Role, string Password, string? PhoneNumber = null)
    : ICommand<Result<StaffAccountDto>>;
