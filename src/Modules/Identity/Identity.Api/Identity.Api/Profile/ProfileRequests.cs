namespace Identity.Api.Profile;

public sealed record ChangeNameRequest(string FullName);

public sealed record ChangePhoneRequest(string? PhoneNumber);

public sealed record ChangeOwnEmailRequest(string Email, string CurrentPassword);

public sealed record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);

public sealed record AvatarResponse(string AvatarUrl);
