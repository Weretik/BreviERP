namespace Identity.Application.Features.Profile.Shared;

public sealed record ProfileDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    string? AvatarUrl,
    string? PhoneNumber);
