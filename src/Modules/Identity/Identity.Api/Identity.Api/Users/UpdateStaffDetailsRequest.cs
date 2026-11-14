namespace Identity.Api.Users;

public sealed record UpdateStaffDetailsRequest(string FullName, string Email, string? PhoneNumber);
