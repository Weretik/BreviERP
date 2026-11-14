namespace Identity.Api.Users;

public sealed record CreateStaffAccountRequest(
    string Email,
    string FullName,
    string Role,
    string Password,
    string? PhoneNumber = null);
