using Identity.Application.Features.Users.Shared;

namespace Identity.Api.Endpoints.Users.Shared;

public sealed record StaffAccountResponse(
    int Id,
    string Email,
    string FullName,
    string Role,
    bool IsBlocked,
    string? AvatarUrl,
    string? PhoneNumber)
{
    public static StaffAccountResponse From(StaffAccountDto dto) =>
        new(dto.Id, dto.Email, dto.FullName, dto.Role, dto.IsBlocked, dto.AvatarUrl, dto.PhoneNumber);
}
