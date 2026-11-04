namespace Identity.Application.Features.Users.UpdateStaffDetails;

/// <summary>Name, email and phone of a staff account, changed by an administrator. Empty phone clears it.</summary>
public sealed record UpdateStaffDetailsCommand(int UserId, string FullName, string Email, string? PhoneNumber) : ICommand<Result>;
