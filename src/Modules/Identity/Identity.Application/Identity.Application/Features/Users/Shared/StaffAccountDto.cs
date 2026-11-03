namespace Identity.Application.Features.Users.Shared;

public sealed record StaffAccountDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    bool IsBlocked);
