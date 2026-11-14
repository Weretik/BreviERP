namespace Identity.Api.Auth;

public sealed record SessionLoginRequest(
    string Email,
    string Password);
