using Identity.Application.Features.Profile.Shared;

namespace Identity.Api.Profile;

public sealed record ProfileResponse(int Id, string Email, string FullName, string Role, string? AvatarUrl, string? PhoneNumber)
{
    public static ProfileResponse From(ProfileDto dto) => new(dto.Id, dto.Email, dto.FullName, dto.Role, dto.AvatarUrl, dto.PhoneNumber);
}

public sealed record AvatarResponse(string AvatarUrl);
