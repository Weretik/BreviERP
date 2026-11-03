using Identity.Application.Features.Auth.SessionMe.DTOs;

namespace Identity.Application.Services;

/// <summary>
/// Browser sign-in with the Identity session cookie.
/// </summary>
public interface ICookieSessionService
{
    Task<SessionMeResult?> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
    Task SignOutAsync(CancellationToken cancellationToken = default);
    Task<SessionMeResult?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
