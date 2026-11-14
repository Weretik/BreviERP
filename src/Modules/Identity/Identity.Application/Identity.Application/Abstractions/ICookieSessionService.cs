using Identity.Application.Features.Auth.Queries.SessionMe;

namespace Identity.Application.Abstractions;

/// <summary>
/// Browser sign-in with the Identity session cookie.
/// </summary>
public interface ICookieSessionService
{
    Task<SessionMeResult?> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
    Task SignOutAsync(CancellationToken cancellationToken = default);
    Task<SessionMeResult?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
